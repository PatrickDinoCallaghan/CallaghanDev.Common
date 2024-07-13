using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace CallaghanDev.Utilities.MathTools
{
    /// <summary>
    /// Represents a fractional number between -1 and 1 with fixed-point decimal handling using a short, 
    /// effectively creating a 2-byte float. While operations with Fraction may be slower than with float 
    /// types, they are typically faster than with decimal types. The Fraction structure is designed to 
    /// offer greater precision for values strictly between -1 and 1 compared to floats after binary operations,
    /// due to its fixed-point decimal handling and specific scale. It balances between medium precision and low memory 
    /// usage while offering medium CPU computational efficiency. In contrast, floats provide high CPU 
    /// computational efficiency and lower precision with medium memory usage, whereas decimals offer high 
    /// precision at the cost of high memory usage and lower CPU computational efficiency.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Fraction : ISerializable
    {
        private long numerator;
        private long denominator;

        #region Constants
        const int DecimalPrecision = 10000;
        const int FractionPrecision = 1000000;
        #endregion

        public double DecimalValue
        {
            get { return (double)numerator / denominator; }
            set
            {
                if (IsIrrational(value))
                {
                    throw new ArgumentException("Cannot convert an irrational number to a fraction.");
                }

                var fraction = FromDecimal((decimal)value);
                numerator = fraction.numerator;
                denominator = fraction.denominator;
            }
        }

        public Fraction(long numerator, long denominator)
        {
            if (denominator == 0)
            {
                throw new ArgumentException("Denominator cannot be zero.");
            }

            this.numerator = numerator;
            this.denominator = denominator;
            Simplify();
        }

        public Fraction(double value)
        {
            if (IsIrrational(value))
            {
                throw new ArgumentException("Cannot convert an irrational number to a fraction.");
            }

            var fraction = FromDecimal((decimal)value);
            numerator = fraction.numerator;
            denominator = fraction.denominator;
        }

        public Fraction(decimal value)
        {
            var fraction = FromDecimal(value);
            numerator = fraction.numerator;
            denominator = fraction.denominator;
        }

        public Fraction(string fractionString)
        {
            if (string.IsNullOrWhiteSpace(fractionString))
            {
                throw new ArgumentException("Input string cannot be null or empty.");
            }

            if (fractionString.Contains("/"))
            {
                // Handle fraction string
                var parts = fractionString.Split('/');
                if (parts.Length != 2)
                {
                    throw new FormatException("Input string is not in the correct format 'numerator/denominator'.");
                }

                if (!long.TryParse(parts[0], out numerator))
                {
                    throw new FormatException("Numerator part is not a valid integer.");
                }

                if (!long.TryParse(parts[1], out denominator))
                {
                    throw new FormatException("Denominator part is not a valid integer.");
                }

                if (denominator == 0)
                {
                    throw new ArgumentException("Denominator cannot be zero.");
                }

                Simplify();
            }
            else
            {
                // Handle decimal string
                if (!decimal.TryParse(fractionString, out decimal decimalValue))
                {
                    throw new FormatException("Input string is not a valid decimal.");
                }

                var fraction = FromDecimal(decimalValue);
                numerator = fraction.numerator;
                denominator = fraction.denominator;
            }
        }

        private void Simplify()
        {
            long gcd = GreatestCommonDivisor(System.Math.Abs(numerator), System.Math.Abs(denominator));
            numerator /= gcd;
            denominator /= gcd;

            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
        }

        private static long GreatestCommonDivisor(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static Fraction FromDecimal(decimal value)
        {
            int sign = System.Math.Sign(value);
            value = System.Math.Abs(value);
            long numerator = (long)(value * FractionPrecision);
            long denominator = FractionPrecision;

            return new Fraction(sign * numerator, denominator);
        }

        private static bool IsIrrational(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return false;
            }

            const double tolerance = 1e-10;
            long denominator = 1;

            while (denominator < long.MaxValue / 10)
            {
                double fraction = System.Math.Round(value * denominator) / denominator;
                if (System.Math.Abs(value - fraction) < tolerance)
                {
                    return false;
                }
                denominator *= 10;
            }

            return true;
        }

        #region Operator Overloads
        public static Fraction operator *(Fraction a, Fraction b)
        {
            return new Fraction(a.numerator * b.numerator, a.denominator * b.denominator);
        }

        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b.numerator == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero fraction.");
            }

            return new Fraction(a.numerator * b.denominator, a.denominator * b.numerator);
        }

        public static Fraction operator +(Fraction a, Fraction b)
        {
            long commonDenominator = a.denominator * b.denominator;
            long numeratorSum = a.numerator * b.denominator + b.numerator * a.denominator;
            return new Fraction(numeratorSum, commonDenominator);
        }

        public static Fraction operator -(Fraction a, Fraction b)
        {
            long commonDenominator = a.denominator * b.denominator;
            long numeratorDifference = a.numerator * b.denominator - b.numerator * a.denominator;
            return new Fraction(numeratorDifference, commonDenominator);
        }

        public static bool operator ==(Fraction a, Fraction b)
        {
            return a.numerator == b.numerator && a.denominator == b.denominator;
        }

        public static bool operator !=(Fraction a, Fraction b)
        {
            return !(a == b);
        }

        public static bool operator >(Fraction a, Fraction b)
        {
            return a.DecimalValue > b.DecimalValue;
        }

        public static bool operator <(Fraction a, Fraction b)
        {
            return a.DecimalValue < b.DecimalValue;
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return $"{numerator}/{denominator}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Fraction other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(numerator, denominator);
        }
        #endregion

        #region Serialization
        private Fraction(SerializationInfo info, StreamingContext context)
        {
            numerator = info.GetInt64("Numerator");
            denominator = info.GetInt64("Denominator");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Numerator", numerator);
            info.AddValue("Denominator", denominator);
        }
        #endregion
    }
}
