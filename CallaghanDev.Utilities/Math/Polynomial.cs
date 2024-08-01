using CallaghanDev.Utilities.MathTools;
using MathNet.Symbolics;
using System.Text;
using System.Text.RegularExpressions;


namespace CallaghanDev.Utilities.Math
{
    public class Polynomial
    {
        // Holds the terms of the polynomial
        private List<Term> terms;

        public Polynomial()
        {
            terms = new List<Term>();
        }

        public Polynomial(string polynomial)
        {
            terms = ParsePolynomial(SimplifyPolynomial(polynomial));
        }

        protected string SimplifyPolynomial(string polynomial)
        {
            // Parse the polynomial string into a symbolic expression
            var expression = Infix.ParseOrThrow(polynomial);

            // Simplify the expression
            var simplifiedExpression = Algebraic.Expand(expression);

            // Convert the simplified expression back to a string
            string simplifiedString = Infix.Format(simplifiedExpression);

            return simplifiedString;
        }

        // Adds a term to the polynomial
        public void AddTerm(double coefficient, int exponent)
        {
            terms.Add(new Term(coefficient, exponent));
        }

        // Evaluates the polynomial at a given value of x
        public double Evaluate(double x)
        {
            double result = 0.0;
            foreach (var term in terms)
            {
                result += term.Coefficient * System.Math.Pow(x, term.Exponent);
            }
            return result;
        }
        private List<Term> ParsePolynomial(string polynomial)
        {
            // Remove spaces for easier parsing
            polynomial = polynomial.Replace(" ", "");

            // Use regex to match each term in the polynomial
            var matches = Regex.Matches(polynomial, @"([+-]?\d*\.?\d*)x\^(\d+)|([+-]?\d*\.?\d*)x|([+-]?\d+\.?\d*)");

            // Initialize list of terms
            List<Term> terms = new List<Term>();

            // Parse each match to fill the terms list
            foreach (Match match in matches)
            {
                if (match.Groups[2].Success)
                {
                    int exponent = int.Parse(match.Groups[2].Value);
                    string coefficientStr = match.Groups[1].Value == "" || match.Groups[1].Value == "+" ? "1" :
                                            match.Groups[1].Value == "-" ? "-1" : match.Groups[1].Value;
                    double coefficient = double.Parse(coefficientStr);
                    terms.Add(new Term(new Fraction(coefficientStr), exponent));
                }
                else if (match.Groups[3].Success)
                {
                    int exponent = 1;
                    string coefficientStr = match.Groups[3].Value == "" || match.Groups[3].Value == "+" ? "1" :
                                            match.Groups[3].Value == "-" ? "-1" : match.Groups[3].Value;
                    double coefficient = double.Parse(coefficientStr);
                    terms.Add(new Term(new Fraction(coefficientStr), exponent));
                }
                else if (match.Groups[4].Success)
                {
                    int exponent = 0;
                    string coefficientStr = match.Groups[4].Value;
                    double constant = double.Parse(coefficientStr);
                    terms.Add(new Term(new Fraction(coefficientStr), exponent));
                }
            }

            return terms;
        }
        public Polynomial Integrate()
        {
            Polynomial integratedPolynomial = new Polynomial();
            foreach (var term in terms)
            {
                double newCoefficient = term.Coefficient / (term.Exponent + 1);
                int newExponent = term.Exponent + 1;
                integratedPolynomial.AddTerm(newCoefficient, newExponent);
            }
            return integratedPolynomial;
        }
        public Polynomial Differentiate()
        {
            Polynomial differentiatedPolynomial = new Polynomial();
            foreach (var term in terms)
            {
                if (term.Exponent == 0)
                {
                    // The derivative of a constant term is zero and can be omitted
                    continue;
                }
                double newCoefficient = term.Coefficient * term.Exponent;
                int newExponent = term.Exponent - 1;
                differentiatedPolynomial.AddTerm(newCoefficient, newExponent);
            }
            return differentiatedPolynomial;
        }
        private class Term
        {
            public double Coefficient { get; }

            private Fraction? _FractionCoefficent;
            public Fraction? FractionCoefficent 
            {
                get
                {
                    if (_FractionCoefficent == null)
                    {

                        try
                        {
                            _FractionCoefficent = new Fraction(Coefficient);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    return _FractionCoefficent;
                }
            }
            public int Exponent { get; }

            public Term(double coefficient, int exponent)
            {
                Coefficient = coefficient;
                Exponent = exponent;
            }

            public Term(Fraction coefficient, int exponent)
            {
                _FractionCoefficent = coefficient;
                Coefficient = coefficient.DecimalValue;
                Exponent = exponent;
            }
        }

        public override string ToString()
        {
            if (terms.Count == 0) return "0";

            var sortedTerms = terms.OrderByDescending(term => term.Exponent).ToList();
            StringBuilder polynomialString = new StringBuilder();

            foreach (var term in sortedTerms)
            {
                if (term.Coefficient == 0) continue;

                string termString = "";
                if (term.Exponent == 0)
                {
                    termString = $"{term.Coefficient}";
                }
                else if (term.Exponent == 1)
                {
                    termString = $"{term.Coefficient}x";
                }
                else
                {
                    termString = $"{term.Coefficient}x^{term.Exponent}";
                }

                if (polynomialString.Length == 0)
                {
                    polynomialString.Append(termString);
                }
                else
                {
                    if (term.Coefficient > 0)
                    {
                        polynomialString.Append($" + {termString}");
                    }
                    else
                    {
                        polynomialString.Append($" - {termString.TrimStart('-')}");
                    }
                }
            }

            return polynomialString.ToString();
        }
    }
        /*
        public string SimplifyPolynomial(string polynomial)
        {
            // Parse the polynomial string into a symbolic expression
            var expression = Infix.ParseOrThrow(polynomial);

            // Simplify the expression
            var simplifiedExpression = Algebraic.Expand(expression);

            // Convert the simplified expression back to a string
            string simplifiedString = Infix.Format(simplifiedExpression);

            return simplifiedString;
        }


        public List<double> ParsePolynomial(string polynomial)
        {
            // Remove spaces for easier parsing
            polynomial = polynomial.Replace(" ", "");

            // Use regex to match each term in the polynomial
            var matches = Regex.Matches(polynomial, @"([+-]?\d*\.?\d*)x\^(\d+)|([+-]?\d*\.?\d*)x|([+-]?\d+\.?\d*)");

            // Determine the degree of the polynomial
            int degree = 0;
            foreach (Match match in matches)
            {
                if (match.Groups[2].Success)
                {
                    degree = System.Math.Max(degree, int.Parse(match.Groups[2].Value));
                }
                else if (match.Groups[3].Success)
                {
                    degree = System.Math.Max(degree, 1);
                }
            }

            // Initialize coefficients list
            List<double> coefficients = new List<double>(new double[degree + 1]);

            // Parse each match to fill the coefficients list
            foreach (Match match in matches)
            {
                if (match.Groups[2].Success)
                {
                    int power = int.Parse(match.Groups[2].Value);
                    double coefficient = double.Parse(match.Groups[1].Value == "" ? "1" : match.Groups[1].Value);
                    coefficients[power] = coefficient;
                }
                else if (match.Groups[3].Success)
                {
                    double coefficient = double.Parse(match.Groups[3].Value == "" ? "1" : match.Groups[3].Value);
                    coefficients[1] = coefficient;
                }
                else if (match.Groups[4].Success)
                {
                    double constant = double.Parse(match.Groups[4].Value);
                    coefficients[0] = constant;
                }
            }

            return coefficients;
        }


        public string IntegratePolynomial(string polynomial)
        {
            // Separate the polynomial into its terms
            List<string> terms = SeparatePolynomialTerms(polynomial);

            // Integrate each term
            List<string> integratedTerms = new List<string>();
            foreach (string term in terms)
            {
                integratedTerms.Add(IntegrateTerm(term));
            }

            // Combine the integrated terms back into a polynomial string
            return string.Join("+", integratedTerms).Replace("+(-", "-(");
        }

        private List<string> SeparatePolynomialTerms(string polynomial)
        {
            // Remove spaces for easier parsing
            polynomial = polynomial.Replace(" ", "");

            // Regular expression pattern to match polynomial terms
            string pattern = @"[+-]?[^-+]+";

            // Use regex to find all matches
            var matches = Regex.Matches(polynomial, pattern);

            // Convert matches to a list of strings
            List<string> terms = new List<string>();
            foreach (Match match in matches)
            {
                terms.Add(match.Value);
            }

            return terms;
        }


        public static string IntegrateTerm(string term)
        {
            // Regular expressions to match different parts of the term
            string coefficientPattern = @"([+-]?\d+(\.\d+)?(/[+-]?\d+(\.\d+)?)?)";
            string variablePattern = @"x(\^(\d+))?";

            // Extract coefficient
            var coefficientMatch = Regex.Match(term, coefficientPattern);
            string coefficientStr = coefficientMatch.Value;
            string coefficient = coefficientStr == "" || coefficientStr == "+" || coefficientStr == "-" ?
                coefficientStr + "1" :
                coefficientStr;

            // Extract variable part
            var variableMatch = Regex.Match(term, variablePattern);
            if (!variableMatch.Success)
            {
                // If there's no variable, it's a constant term (x^0)
                return $"{coefficient}x";
            }

            // Extract exponent
            int exponent = 1;
            if (variableMatch.Groups[2].Success)
            {
                exponent = int.Parse(variableMatch.Groups[2].Value);
            }

            // Calculate new exponent
            int newExponent = exponent + 1;

            // Return the integrated term
            if (newExponent == 1)
            {
                return $"{coefficient}x";
            }
            return $"{DivideFraction(coefficient, newExponent)}x^{newExponent}";
        }

        public static string DivideFraction(string fraction, int divisor)
        {
            if (fraction.Contains("/"))
            {
                var parts = fraction.Split('/');
                int numerator = int.Parse(parts[0]);
                int denominator = int.Parse(parts[1]) * divisor;
                return $"({numerator}/{denominator})";
            }
            else
            {
                int numerator = int.Parse(fraction);
                return $"{numerator}/{divisor}";
            }
        }
        */
}
