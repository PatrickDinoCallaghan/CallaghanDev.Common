using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;
using CallaghanDev.Common.Math;
using ILGPU.IR.Values;
using System.Text.RegularExpressions;
using Microsoft.FSharp.Collections;
using ILGPU.Runtime.Cuda;

namespace CallaghanDev.Utilities.Math
{
    public sealed class SuperscriptDictionary
    {
        private static readonly Lazy<SuperscriptDictionary> lazy = new Lazy<SuperscriptDictionary>(() => new SuperscriptDictionary());

        public static SuperscriptDictionary Instance { get { return lazy.Value; } }

        public Dictionary<int, char> Dictionary { get; private set; }

        public const char RootSymbol = '\u221A';

        private SuperscriptDictionary()
        {
            Dictionary = new Dictionary<int, char>
            {
                { 0, '\u2070' },
                { 1, '\u00B9' },
                { 2, '\u00B2' },
                { 3, '\u00B3' },
                { 4, '\u2074' },
                { 5, '\u2075' },
                { 6, '\u2076' },
                { 7, '\u2077' },
                { 8, '\u2078' },
                { 9, '\u2079' }
            };
        }

        public static string ConvertToSuperscript(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                sb.Append(GetSuperscript(c));
            }
            return sb.ToString();
        }

        public static char GetSuperscript(char c)
        {
            var superscriptDict = SuperscriptDictionary.Instance.Dictionary;

            if (char.IsDigit(c))
            {
                int digit = c - '0'; // Convert char digit to int
                if (superscriptDict.TryGetValue(digit, out char superscriptChar))
                {
                    return superscriptChar;
                }
            }
            else if (superscriptDict.TryGetValue(c, out char superscriptChar))
            {
                return superscriptChar;
            }
            return c;
        }

        public static string CreateRootWithBase(int n)
        {
            var superscriptDict = SuperscriptDictionary.Instance.Dictionary;
            StringBuilder sb = new StringBuilder();

            if (n > 2) // Square root usually omits the base if it is 2
            {
                string baseStr = n.ToString();
                foreach (char c in baseStr)
                {
                    if (char.IsDigit(c))
                    {
                        int digit = c - '0';
                        if (superscriptDict.TryGetValue(digit, out char superscriptChar))
                        {
                            sb.Append(superscriptChar);
                        }
                    }
                }
            }
            sb.Append(RootSymbol);
            return sb.ToString();
        }

    }

    public static class BigIntegerExtensions
    {
        public static bool TryNthRoot(BigInteger number, int root, out BigInteger result)
        {
            if (root <= 1)
            {
                throw new ArgumentException("Root must be greater than 1.", nameof(root));
            }

            if (number < 0 && root % 2 == 0)
            {
                throw new ArgumentException("Cannot compute even root of a negative number.");
            }

            if (number == 0)
            {
                result = 0;
                return true;
            }

            BigInteger x0 = number;
            BigInteger x1 = (number / root + BigInteger.One) / root;

            while (x1 < x0)
            {
                x0 = x1;
                BigInteger divisor = BigInteger.Pow(x1, root - 1);

                // Check for zero divisor to avoid DivideByZeroException
                if (divisor.IsZero)
                {
                    result = 0;
                    return false;
                }

                x1 = ((root - 1) * x1 + number / divisor) / root;
            }

            result = x0;

            // Verify if the result is an exact integer root
            return BigInteger.Pow(result, root) == number;
        }

    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class Scalar : ISerializable
    {
        private BigInteger _numerator;
        private BigInteger _denominator;
        private int _integerPart;

        public int RootBase { get; private set; } = 1;

        public BigInteger Numerator
        {
            get => _numerator;
            private set => _numerator = value;
        }

        public BigInteger Denominator
        {
            get => _denominator;
            private set
            {
                if (value == 0)
                    throw new ArgumentException("Denominator cannot be zero.");
                _denominator = value;
            }
        }

        public int IntegerPart
        {
            get => _integerPart;
            private set => _integerPart = value;
        }

        public double DecimalValue
        {
            get
            {
                if (RootBase == 1)
                {
                    return (double)_integerPart + ((double)_numerator / (double)_denominator);
                }
                if (RootBase == 2)
                {
                    return System.Math.Sqrt((double)_integerPart + ((double)_numerator / (double)_denominator));

                }
                else if (RootBase == 3)
                {
                    return System.Math.Cbrt((double)_integerPart + ((double)_numerator / (double)_denominator));
                }
                else
                {
                    return System.Math.Pow((double)_integerPart + ((double)_numerator / (double)_denominator), (1 / (double)RootBase));
                }
            }
            set
            {
                var fraction = FromDecimalValue(value);
                _numerator = fraction._numerator;
                _denominator = fraction._denominator;
                _integerPart = fraction._integerPart;
            }
        }

        public Scalar(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0)
                throw new ArgumentException("Denominator cannot be zero.");

            _integerPart = 0;
            _numerator = numerator;
            _denominator = denominator;
            if (numerator > denominator)
            {
                Simplify();
            }
        }

        public Scalar(double value)
        {
            Scalar fraction = FromDecimalValue(value);

            RootBase = fraction.RootBase;
            IntegerPart = fraction.IntegerPart;
            Denominator = fraction.Denominator;
            Numerator = fraction.Numerator;
        }
        public Scalar(int value)
        {
            _numerator = 0;
            _denominator = 1;
            _integerPart = value;
        }

        public Scalar(decimal value)
        {
            var fraction = FromDecimalValue(value);
            _numerator = fraction._numerator;
            _denominator = fraction._denominator;
            _integerPart = fraction._integerPart;
        }

        public Scalar(string fractionString)
        {
            Scalar temp = FromString(fractionString);

            _numerator = temp.Numerator;
            _denominator = temp.Denominator;
            RootBase = temp.RootBase;
            _integerPart = temp.IntegerPart;
        }
        private Scalar(int integerPart, BigInteger numerator, BigInteger denominator)
        {
            _integerPart = integerPart;
            _numerator = numerator;
            _denominator = denominator;
            Simplify();
        }

        private static Scalar FromString(string fractionString)
        {
            bool negative = false;
            if (string.IsNullOrWhiteSpace(fractionString))
                throw new ArgumentException("Input string cannot be null or empty.");

            if (fractionString[0] == '+')
            {
                fractionString= fractionString.Substring(1);
            }
            else if (fractionString[0] == '-')
            {
                fractionString = fractionString.Substring(1);
                negative = true;
            }
            BigInteger numerator;
            BigInteger denominator;
            int integerPart = 0;

            string[] parts;

            fractionString = fractionString.Replace(" ", "").Replace("(", "").Replace(")", "");

            if (fractionString.Contains("+"))
            {
                var mainParts = fractionString.Split('+');
                if (mainParts.Length != 2)
                    throw new FormatException("Input string is not in the correct format 'integerPart*(numerator/denominator)'.");

                if (!int.TryParse(mainParts[0], out integerPart))
                    throw new FormatException("Integer part is not a valid integer.");

                // Remove parentheses from the fractional part
                string fractionPart = mainParts[1].Replace('(', ')');
                parts = fractionPart.Split('/');
            }
            else if (fractionString.Contains("/"))
            {
                parts = fractionString.Split('/');
            }
            else
            {
                if (!decimal.TryParse(fractionString, out decimal decimalValue))
                    throw new FormatException("Input string is not a valid decimal.");

                var fraction = FromDecimalValue(decimalValue);
                return fraction;
            }

            if (parts.Length != 2)
                throw new FormatException("Input string is not in the correct format 'numerator/denominator'.");

            if (!BigInteger.TryParse(parts[0], out numerator) || !BigInteger.TryParse(parts[1], out denominator))
                throw new FormatException("Numerator or denominator part is not a valid integer.");

            if (denominator == 0)
                throw new ArgumentException("Denominator cannot be zero.");

            if(negative)
            {
                if (integerPart != 0)
                {
                    integerPart = integerPart * -1;
                }
                else { numerator = numerator *-1; }
            }
            return new Scalar(integerPart, numerator, denominator);
        }

        private void Simplify()
        {
            // Convert the integer part to a fraction and add it to the numerator
            if (_integerPart != 0)
            {
                _numerator += _integerPart * _denominator;
                _integerPart = 0;
            }

            // Adjust the integer part if the numerator is greater than or equal to the denominator
            if (_numerator >= _denominator || _numerator <= -_denominator)
            {
                _integerPart += (int)(_numerator / _denominator);
                _numerator %= _denominator;
            }

            // Simplify the fraction by dividing by the greatest common divisor
            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(_numerator), BigInteger.Abs(_denominator));
            _numerator /= gcd;
            _denominator /= gcd;

            // Ensure the denominator is positive
            if (_denominator < 0)
            {
                _numerator = -_numerator;
                _denominator = -_denominator;
            }
        }
        private static Scalar FromDecimalValue(decimal value)
        {
            return Scalar.FromDecimalValue((double)value);
        }
        private static Scalar FromDecimalValue(double value)
        {
            // Handle NaN or Infinity inputs
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Cannot convert NaN or Infinity to a fraction.");

            // Separate the integer and fractional parts
            int integerPart = (int)System.Math.Truncate(value);
            double fractionalPart = System.Math.Abs(value - integerPart);

            // If there is no fractional part, return a fraction with only the integer part
            if (fractionalPart == 0)
            {
                return new Scalar(integerPart, 0, 1); // Integer part with numerator 0 and denominator 1
            }

            // Continued fraction approximation
            BigInteger numerator = 1;
            BigInteger prevNumerator = 0;
            BigInteger denominator = 0;
            BigInteger prevDenominator = 1;
            double remaining = fractionalPart;
            const double tolerance = 1e-10;

            // Use continued fraction to approximate the fractional part
            while (System.Math.Abs(fractionalPart - ((double)numerator / (double)denominator)) > tolerance)
            {
                int intPart = (int)System.Math.Floor(remaining);
                BigInteger tempNumerator = numerator;
                BigInteger tempDenominator = denominator;

                numerator = intPart * numerator + prevNumerator;
                prevNumerator = tempNumerator;

                denominator = intPart * denominator + prevDenominator;
                prevDenominator = tempDenominator;

                remaining = 1.0 / (remaining - intPart);

                // Break if denominator becomes too large
                if (denominator > BigInteger.Pow(10, 6))
                    break;
            }

            return new Scalar(integerPart, numerator, denominator);
        }

        #region Operator Overloads
        public static Scalar operator *(Scalar a, Scalar b)
        {
            if (a.RootBase == 1 && b.RootBase == 1)
            {
                BigInteger Numerator = ((a.IntegerPart * a.Denominator) + a.Numerator) * ((b.IntegerPart * b.Denominator) + b.Numerator);
                BigInteger Denominator = a.Denominator * b.Denominator;
                return new Scalar(Numerator, Denominator);
            }
            else
            {

                return new Scalar(a.DecimalValue * b.DecimalValue);
            }

        }

        public static Scalar operator /(Scalar a, Scalar b)
        {
            if (b.DecimalValue == 0)
                throw new DivideByZeroException("Cannot divide by zero fraction.");

            Scalar b_inverse = new Scalar(b.Denominator, (b.Numerator + (b.IntegerPart * b.Denominator)));

            return a * b_inverse;
        }

        public static Scalar operator +(Scalar a, Scalar b)
        {
            if (a.RootBase == 1 && b.RootBase == 1)
            {
                BigInteger Numerator = (a.Numerator * b.Denominator) + (b.Numerator * a.Denominator);
                BigInteger Denominator = a.Denominator * b.Denominator;
                return new Scalar(a.IntegerPart + b.IntegerPart, Numerator, Denominator);
            }
            else
            {
                return new Scalar(a.DecimalValue + b.DecimalValue);
            }
        }

        public static Scalar operator -(Scalar a, Scalar b)
        {
            if (a.RootBase == 1 && b.RootBase == 1)
            {

                BigInteger Numerator = (a.Numerator * b.Denominator) - (b.Numerator * a.Denominator);
                BigInteger Denominator = a.Denominator * b.Denominator;
                return new Scalar(a.IntegerPart - b.IntegerPart, Numerator, Denominator);
            }
            else
            {
                return new Scalar(a.DecimalValue - b.DecimalValue);
            }
        }

        public static bool operator ==(Scalar a, Scalar b)
        {
            if (a.IntegerPart == b.IntegerPart && a.Numerator == b.Numerator && b.Denominator == a.Denominator && a.RootBase == b.RootBase)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(Scalar a, Scalar b)
        {
            return !(a == b);
        }

        public static bool operator >(Scalar a, Scalar b)
        {
            return (a.DecimalValue > b.DecimalValue);
        }

        public static bool operator <(Scalar a, Scalar b)
        {
            if (a.IntegerPart < b.IntegerPart || a.IntegerPart > b.IntegerPart)
            {
                return (a.IntegerPart < b.IntegerPart);
            }
            else
            {
                return a.DecimalValue < b.DecimalValue;
            }
        }

        public static Scalar operator ^(Scalar a, int pow)
        {
            if (pow > 0)
            {
                BigInteger Temp_Numerator = BigInteger.Pow(a.IntegerPart * a.Denominator + a.Numerator, pow);
                BigInteger Temp_Denominator = BigInteger.Pow(a.Denominator, pow);
                return new Scalar(Temp_Numerator, Temp_Denominator);
            }
            else if (pow == 0)
            {
                return new Scalar(1, 0, 1);
            }
            else
            {
                BigInteger Temp_Numerator = BigInteger.Pow(a.IntegerPart * a.Denominator + a.Numerator, pow);
                BigInteger Temp_Denominator = BigInteger.Pow(a.Denominator, pow);
                return new Scalar(Temp_Denominator, Temp_Numerator);
            }
        }

        public static Scalar operator ^(Scalar a, double pow)
        {
            double PowerVal = System.Math.Pow(a.DecimalValue, pow);


            return new Scalar(PowerVal);
        }

        public static Scalar operator ^(Scalar a, Scalar pow)
        {

            BigInteger Temp_PowNumerator = pow.IntegerPart * pow.Denominator + pow.Numerator;
            BigInteger Temp_Denominator;
            if ((BigInteger)a.RootBase > 0)
            {
                Temp_Denominator = pow.Denominator * (BigInteger)a.RootBase;
            }
            else
            {
                Temp_Denominator = pow.Denominator;
            }
            if (Temp_PowNumerator % Temp_Denominator == 0)
            {
                Temp_PowNumerator = Temp_PowNumerator / Temp_Denominator;

                Temp_Denominator = 1;
            }
            Scalar returnFraction;

            if (BigIntegerExtensions.TryNthRoot(a.IntegerPart * a.Denominator + a.Numerator, (int)pow.Denominator, out BigInteger NumeratorSqrt) && BigIntegerExtensions.TryNthRoot(a.Denominator, (int)pow.Denominator, out BigInteger DenominatorSqrt) && (int)pow.Denominator > 1)
            {
                returnFraction = new Scalar(BigInteger.Pow(NumeratorSqrt, (int)Temp_PowNumerator), BigInteger.Pow(DenominatorSqrt, (int)Temp_PowNumerator));
                returnFraction.RootBase = a.RootBase;
            }
            else
            {
                returnFraction = a ^ (int)Temp_PowNumerator;
                returnFraction.RootBase = (int)Temp_Denominator;
            }

            return returnFraction;
        }


        public static implicit operator Scalar(int value)
        {
            return new Scalar(value, 1);
        }

        public static implicit operator Scalar(double value)
        {
            return new Scalar(value);
        }

        public static implicit operator Scalar(decimal value)
        {
            return new Scalar(value);
        }

        public static implicit operator Scalar(string value)
        {
            return FromString(value);
        }


        #endregion

        #region Public Methods
        public override string ToString()
        {
            if (RootBase > 1)
            {
                return $"({SuperscriptDictionary.CreateRootWithBase((int)RootBase)}({InnerString()}))";
            }

            return $"({InnerString()})";
        }

        private string InnerString()
        {
            if (_integerPart == 0)
            {
                if (_numerator == 0)
                {
                    return "0";
                }
                else
                {
                    return $"{_numerator}/{_denominator}";
                }
            }
            else if (_numerator == 0)
            {
                return $"{_integerPart}";
            }
            else
            {
                return $"{_integerPart}+({_numerator}/{_denominator})";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Scalar other)
                return this == other;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_integerPart, _numerator, _denominator);
        }
        #endregion

        #region Serialization
        private Scalar(SerializationInfo info, StreamingContext context)
        {
            _integerPart = info.GetInt32("IntegerPart");
            _numerator = (BigInteger)info.GetValue("Numerator", typeof(BigInteger));
            _denominator = (BigInteger)info.GetValue("Denominator", typeof(BigInteger));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IntegerPart", _integerPart);
            info.AddValue("Numerator", _numerator);
            info.AddValue("Denominator", _denominator);
        }
        #endregion
    }

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
        public Dictionary<PolynomialDegree, Scalar> Terms { get; set; } = new Dictionary<PolynomialDegree, Scalar>();

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

            return builder.Length > 0 ? builder.ToString() : "0";
        }

        public void Integrate()
        {
            var integratedTerms = new Dictionary<PolynomialDegree, Scalar>();

            foreach (var term in Terms)
            {
                var degree = term.Key.Degree + 1;
                var coefficient = term.Value / degree;
                integratedTerms[new PolynomialDegree(degree)] = coefficient;
            }

            Terms = integratedTerms;
        }

        public void Differentiate()
        {
            var differentiatedTerms = new Dictionary<PolynomialDegree, Scalar>();

            foreach (var term in Terms)
            {
                if (term.Key.Degree == 0)
                    continue;

                var degree = term.Key.Degree - 1;
                var coefficient = term.Value * term.Key.Degree;
                differentiatedTerms[new PolynomialDegree(degree)] = coefficient;
            }

            Terms = differentiatedTerms;
        }
    }
}