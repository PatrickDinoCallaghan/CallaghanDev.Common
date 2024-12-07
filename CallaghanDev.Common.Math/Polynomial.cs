using CallaghanDev.Common.Math;
using CallaghanDev.Utilities.MathTools;
using MathNet.Symbolics;
using System.Collections.Concurrent;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;


namespace CallaghanDev.Utilities.Math
{
    public struct PolynomialDegree
    {
        public int Degree { get; set; }

        // Constructor for easy initialization
        public PolynomialDegree(int degree)
        {
            Degree = degree;
        }

        // Implicit conversion from int to PolynomialDegree
        public static implicit operator PolynomialDegree(int degree)
        {
            return new PolynomialDegree(degree);
        }

        // Implicit conversion from PolynomialDegree to int
        public static implicit operator int(PolynomialDegree degree)
        {
            return degree.Degree;
        }

        // Override ToString for better debugging and readability
        public override string ToString()
        {
            return Degree.ToString();
        }
    }

    //All functions will be kept in expanded form
    public class PolynomialFunction
    {
        public char Variable { get; set; } = 'x';
        private bool Constant { get; set; }
        private ConcurrentDictionary<PolynomialDegree, Scalar> Terms { get; set; } = new ConcurrentDictionary<PolynomialDegree, Scalar>();

        public static implicit operator PolynomialFunction(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Input string cannot be null or whitespace.", nameof(value));

            var polynomial = new PolynomialFunction();
            value = value.Replace(" ", ""); // Remove all whitespace
            var terms = Regex.Split(value, @"(?=[+-])"); // Split by '+' or '-' keeping the operators

            foreach (var term in terms)
            {
                if (string.IsNullOrWhiteSpace(term))
                    continue;

                // Regex pattern to extract coefficient, variable, and degree
                var match = Regex.Match(term, @"^([+-]?(\(?\d*\.?\d+|\(?\d+/\d+\)?|[+-]))?\*?([a-zA-Z])?\^?(\d+)?$");
                if (!match.Success)
                    throw new FormatException($"Invalid term format: '{term}'");

                // Extract coefficient
                string coefficientPart = match.Groups[1].Value;
                Scalar coefficient = coefficientPart switch
                {
                    "+" => 1,
                    "-" => -1,
                    "" => 1,
                    _ => new Scalar(coefficientPart)
                };

                // Extract variable and degree
                string variablePart = match.Groups[3].Value;
                string degreePart = match.Groups[4].Value;

                PolynomialDegree degree = 0;
                if (!string.IsNullOrEmpty(variablePart))
                {
                    // Ensure all variables in the polynomial are consistent
                    if (polynomial.Variable != null && variablePart != polynomial.Variable.ToString())
                        throw new ArgumentException($"Unexpected variable '{variablePart}' in term '{term}'. Expected variable '{polynomial.Variable}'.");

                    // Assign the variable name if it hasn't been set
                    if (polynomial.Variable == null)
                        polynomial.Variable = variablePart[0];

                    degree = string.IsNullOrEmpty(degreePart) ? 1 : int.Parse(degreePart);
                }

                // Add or update the term in the polynomial
                if (polynomial.Terms.ContainsKey(degree))
                    polynomial.Terms[degree] += coefficient;
                else
                    polynomial.Terms[degree] = coefficient;
            }

            return polynomial;
        }

        public static PolynomialFunction operator +(PolynomialFunction left, PolynomialFunction right)
        {
            if (right.Variable != left.Variable)
                throw new Exception("Polynomial functions must have the same variable.");

            var combined = new ConcurrentDictionary<PolynomialDegree, Scalar>(left.Terms);

            // Use Parallel.ForEach to iterate through the right terms
            Parallel.ForEach(right.Terms, term =>
            {
                combined.AddOrUpdate(
                    term.Key,
                    term.Value,
                    (key, existingValue) => existingValue + term.Value);
            });

            return new PolynomialFunction { Terms = combined };
        }
        public static PolynomialFunction operator -(PolynomialFunction left, PolynomialFunction right)
        {
            if (right.Variable != left.Variable)
                throw new Exception("Polynomial functions must have the same variable.");

            var combined = new ConcurrentDictionary<PolynomialDegree, Scalar>(left.Terms);

            // Use Parallel.ForEach to iterate through the right terms
            Parallel.ForEach(right.Terms, term =>
            {
                combined.AddOrUpdate(
                    term.Key,
                    term.Value,
                    (key, existingValue) => existingValue - term.Value);
            });

            return new PolynomialFunction { Terms = combined };
        }

        public Scalar Evaluate(double x)
        {
            return Evaluate((decimal)x);
        }
        public Scalar Evaluate(int x)
        {
            return Evaluate((decimal)x);
        }
        public Scalar Evaluate(decimal x)
        {
            ConcurrentDictionary<PolynomialDegree, Scalar> partialResults = new ConcurrentDictionary<PolynomialDegree, Scalar>();

            // Divide the dictionary into chunks for parallel processing
            var partitions = Partitioner.Create(Terms).GetPartitions(Environment.ProcessorCount);

            // Process each partition in parallel
            Parallel.ForEach(partitions, (partition, state, partitionIndex) =>
            {
                Scalar localSum = 0;

                // Process the terms in this partition
                while (partition.MoveNext())
                {
                    var term = partition.Current;
                    localSum += term.Value * x.Pow(term.Key);
                }

                // Store the partial result for this partition
                partialResults[(int)partitionIndex] = localSum;
            });

            // Aggregate the results from all partitions
            Scalar total = 0;

            foreach (var result in partialResults)
            {
                total += result.Value;
            }

            return total;
        }
        public PolynomialFunction Differentiate()
        {
            var differentiatedTerms = new ConcurrentDictionary<PolynomialDegree, Scalar>();

            Parallel.ForEach(Terms, term =>
            {
                if (term.Key.Degree == 0)
                    return;
                var degree = term.Key.Degree - 1;
                var coefficient = term.Value * term.Key.Degree;
                differentiatedTerms[new PolynomialDegree(degree)] = coefficient;
            });

            return new PolynomialFunction() { Terms = differentiatedTerms, Constant = false };
        }
        public PolynomialFunction Integrate()
        {
            var integratedTerms = new ConcurrentDictionary<PolynomialDegree, Scalar>();

            Parallel.ForEach(Terms, term =>
            {
                var degree = term.Key.Degree + 1;
                var coefficient = term.Value / degree;
                integratedTerms[new PolynomialDegree(degree)] = coefficient;
            });

            return new PolynomialFunction() { Terms = integratedTerms, Constant = true };
        }
        public Scalar Integrate(decimal Upper, decimal Lower)
        {
            PolynomialFunction polynomialFunction = Integrate();
            return polynomialFunction.Evaluate(Upper) - polynomialFunction.Evaluate(Lower);
        }
        public override string ToString()
        {
            if (Terms == null || Terms.Count == 0)
                return "0";

            var builder = new StringBuilder();

            foreach (var term in Terms.OrderByDescending(t => t.Key.Degree))
            {
                var degree = term.Key;
                var coefficient = term.Value;

                if (coefficient == 0)
                    continue;

                var sign = coefficient > 0 && builder.Length > 0 ? "+" : "";

                if (degree.Degree == 0)
                {
                    builder.Append($"{sign}{coefficient}");
                }
                else if (degree.Degree == 1)
                {
                    builder.Append($"{sign}{(coefficient == 1 ? "" : coefficient.ToString())}{Variable}");
                }
                else
                {
                    builder.Append($"{sign}{(coefficient == 1 ? "" : coefficient.ToString())}{Variable}^{degree.Degree}");
                }
            }
            if (Constant)
            {
                builder.Append("+C");
            }

            return builder.Length > 0 ? builder.ToString() : "0";
        }
    }
}
