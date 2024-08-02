using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;

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

            if (n != 2) // Square root usually omits the base if it is 2
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
            if (number < 0 && root % 2 == 0)
            {
                throw new ArgumentException("Cannot compute even root of a negative number.");
            }

            if (number == 0)
            {
                result = 0;
                return true;
            }

            BigInteger n = number;
            BigInteger x0 = number;
            BigInteger x1 = (BigInteger.Divide(n, root) + n) / root;

            while (x1 < x0)
            {
                x0 = x1;
                x1 = (BigInteger.Divide(BigInteger.Multiply(x1, root - 1) + BigInteger.Divide(n, BigInteger.Pow(x1, root - 1)), root));
            }

            result = x0;

            // Verify if the result is an exact integer root
            return BigInteger.Pow(result, root) == number;
        }
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class Fraction : ISerializable
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
                return System.Math.Pow((double)_integerPart + ((double)_numerator / (double)_denominator), (1 / (double)RootBase));
            }
            set
            {
                if (IsIrrational(value))
                    throw new ArgumentException("Cannot convert an irrational number to a fraction.");

                var fraction = FromDecimal((decimal)value);
                _numerator = fraction._numerator;
                _denominator = fraction._denominator;
                _integerPart = fraction._integerPart;
            }
        }

        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0)
                throw new ArgumentException("Denominator cannot be zero.");

            _integerPart = 0;
            _numerator = numerator;
            _denominator = denominator;
            Simplify();
        }

        public Fraction(double value)
        {
            if (IsIrrational(value))
                throw new ArgumentException("Cannot convert an irrational number to a fraction.");

            var fraction = FromDecimal((decimal)value);
            _numerator = fraction._numerator;
            _denominator = fraction._denominator;
            _integerPart = fraction._integerPart;
        }
        public Fraction(int value)
        {
            _numerator = 0;
            _denominator =1;
            _integerPart = value;
        }


        public Fraction(decimal value)
        {
            var fraction = FromDecimal(value);
            _numerator = fraction._numerator;
            _denominator = fraction._denominator;
            _integerPart = fraction._integerPart;
        }

        public Fraction(string fractionString)
        {
            Fraction temp = FromString(fractionString);

            _numerator = temp.Numerator;
            _denominator = temp.Denominator;
            RootBase = temp.RootBase;
            _integerPart = temp.IntegerPart;
        }
        private Fraction(int integerPart, BigInteger numerator, BigInteger denominator)
        {
            _integerPart = integerPart;
            _numerator = numerator;
            _denominator = denominator;
            Simplify();
        }

        private static Fraction FromString(string fractionString)
        {
            if (string.IsNullOrWhiteSpace(fractionString))
                throw new ArgumentException("Input string cannot be null or empty.");

            BigInteger numerator;
            BigInteger denominator;
            int integerPart = 0;

            string[] parts;

            if (fractionString.Contains(","))
            {
                var mainParts = fractionString.Split(',');
                if (mainParts.Length != 2)
                    throw new FormatException("Input string is not in the correct format 'integerPart*(numerator/denominator)'.");

                if (!int.TryParse(mainParts[0], out integerPart))
                    throw new FormatException("Integer part is not a valid integer.");

                // Remove parentheses from the fractional part
                string fractionPart = mainParts[1].Trim('(', ')');
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

                var fraction = FromDecimal(decimalValue);
                return fraction;
            }

            if (parts.Length != 2)
                throw new FormatException("Input string is not in the correct format 'numerator/denominator'.");

            if (!BigInteger.TryParse(parts[0], out numerator) || !BigInteger.TryParse(parts[1], out denominator))
                throw new FormatException("Numerator or denominator part is not a valid integer.");

            if (denominator == 0)
                throw new ArgumentException("Denominator cannot be zero.");

            return new Fraction(integerPart, numerator, denominator);
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

        public static Fraction FromDecimal(decimal value)
        {
            int sign = System.Math.Sign(value);
            value = System.Math.Abs(value);

            int[] bits = decimal.GetBits(value);
            BigInteger numerator = (BigInteger)((long)bits[2] << 32 | bits[1]) << 32 | (uint)bits[0];
            BigInteger denominator = BigInteger.Pow(10, bits[3] >> 16);

            int integerPart = (int)(numerator / denominator);
            numerator %= denominator;

            numerator *= sign;

            return new Fraction(integerPart, numerator, denominator);
        }

        private static bool IsIrrational(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return false;

            const double tolerance = 1e-10;
            BigInteger denominator = 1;

            while (denominator < BigInteger.Pow(10, 100))
            {
                double fraction = System.Math.Round(value * (double)denominator) / (double)denominator;
                if (System.Math.Abs(value - fraction) < tolerance)
                    return false;
                denominator *= 10;
            }

            return true;
        }

        #region Operator Overloads
        public static Fraction operator *(Fraction a, Fraction b)
        {
            int NewIntegerPart = a.IntegerPart * b.IntegerPart;
            BigInteger numerator = a.Numerator * b.Numerator;
            BigInteger denominator = a.Denominator * b.Denominator;

            return new Fraction(NewIntegerPart, numerator, denominator);
        }

        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b.DecimalValue == 0)
                throw new DivideByZeroException("Cannot divide by zero fraction.");

            int NewIntegerPart = a.IntegerPart * b.IntegerPart;
            BigInteger numerator = a.Numerator * b.Denominator + b.Numerator * a.Denominator;
            BigInteger denominator = a.Denominator * b.Denominator;

            return new Fraction(NewIntegerPart, numerator, denominator);
        }

        public static Fraction operator +(Fraction a, Fraction b)
        {
            return new Fraction(a.DecimalValue + b.DecimalValue);
        }

        public static Fraction operator -(Fraction a, Fraction b)
        {
            return new Fraction(a.DecimalValue - b.DecimalValue);
        }

        public static bool operator ==(Fraction a, Fraction b)
        {
            if (a.IntegerPart == b.IntegerPart && a.Numerator == b.Numerator && b.Denominator == a.Denominator)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(Fraction a, Fraction b)
        {
            return !(a == b);
        }

        public static bool operator >(Fraction a, Fraction b)
        {
            if (a.IntegerPart > b.IntegerPart || a.IntegerPart < b.IntegerPart)
            {
                return (a.IntegerPart > b.IntegerPart);
            }
            else
            {
                return a.DecimalValue > b.DecimalValue;
            }
        }

        public static bool operator <(Fraction a, Fraction b)
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

        public static Fraction operator ^(Fraction a, int pow)
        {
            if (pow > 0)
            {
                BigInteger Temp_Numerator = BigInteger.Pow(a.IntegerPart * a.Denominator + a.Numerator, pow);
                BigInteger Temp_Denominator = BigInteger.Pow(a.Denominator, pow);
                return new Fraction(Temp_Numerator, Temp_Denominator);
            }
            else if (pow == 0)
            {
                return new Fraction(1, 0, 1);
            }
            else
            {
                BigInteger Temp_Numerator = BigInteger.Pow(a.IntegerPart * a.Denominator + a.Numerator, pow);
                BigInteger Temp_Denominator = BigInteger.Pow(a.Denominator, pow);
                return new Fraction(Temp_Denominator, Temp_Numerator);
            }
        }

        public static Fraction operator ^(Fraction a, double pow)
        {
            double PowerVal = System.Math.Pow(a.DecimalValue, pow);


            return new Fraction(PowerVal);
        }

        public static Fraction operator ^(Fraction a, Fraction pow)
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
            Fraction returnFraction;

            if (BigIntegerExtensions.TryNthRoot(a.IntegerPart * a.Denominator + a.Numerator, (int)pow.Denominator, out BigInteger NumeratorSqrt) && BigIntegerExtensions.TryNthRoot(a.Denominator, (int)pow.Denominator, out BigInteger DenominatorSqrt) && (int)pow.Denominator > 1)
            {
                returnFraction = new Fraction(BigInteger.Pow(NumeratorSqrt, (int)Temp_PowNumerator), BigInteger.Pow(DenominatorSqrt, (int)Temp_PowNumerator));
                returnFraction.RootBase = a.RootBase;
            }
            else
            {
                returnFraction = a ^ (int)Temp_PowNumerator;
                returnFraction.RootBase = (int)Temp_Denominator;
            }

            return returnFraction;
        }


        public static implicit operator Fraction(int value)
        {
            return new Fraction(value, 1);
        }

        public static implicit operator Fraction(double value)
        {
            return new Fraction(value);
        }

        public static implicit operator Fraction(string value)
        {
            return FromString(value);
        }

 
        #endregion

        #region Public Methods
        public override string ToString()
        {
              if (RootBase != 0)
                {
                    return $"{SuperscriptDictionary.CreateRootWithBase((int)RootBase)}({InnerString()})"; 
                }

            return InnerString();
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
                return $"{_integerPart},({_numerator}/{_denominator})";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Fraction other)
                return this == other;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_integerPart, _numerator, _denominator);
        }
        #endregion

        #region Serialization
        private Fraction(SerializationInfo info, StreamingContext context)
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

    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
    public class MixedNumber
    {
        public Fraction Term;

        private List<MixedNumber> Terms;

        public Operator? Operator;
    }
}