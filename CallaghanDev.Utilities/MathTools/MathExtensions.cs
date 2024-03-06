using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.MathTools
{
    public static class MathExtensions
    {
        public static decimal Sq(this decimal value)
        {
            return value * value;
        }

        public static float Sq(this float value)
        {
            return value * value;
        }

        public static int Sq(this int value)
        {
            return value * value;
        }

        public static long Sq(this long value)
        {
            return value * value;
        }

        public static BigInteger Sqrt(this BigInteger value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
            if (value == 0) return BigInteger.Zero;

            BigInteger n = value / 2 + 1; // Initial guess
            BigInteger n1 = (n + value / n) / 2;

            while (n1 < n)
            {
                n = n1;
                n1 = (n + value / n) / 2;
            }

            return n;
        }

        public static BigInteger Ceiling(this BigInteger value, BigInteger divisor)
        {
            return (value + divisor - 1) / divisor;
        } 

        public static BigInteger Floor(this BigInteger value, BigInteger divisor)
        {
            return value / divisor;
        }

        public static BigInteger Round(this BigInteger value)
        {
            // Check if the value is negative
            bool isNegative = value < 0;
            BigInteger absValue = BigInteger.Abs(value);

            // Get the decimal part (i.e., remainder when dividing by 1)
            BigInteger remainder = absValue % 10;

            // If the remainder is 5 or more, round up; otherwise, round down
            if (remainder >= 5)
            {
                absValue += (10 - remainder); // Adjust for rounding up
            }
            else
            {
                absValue -= remainder; // Adjust for rounding down
            }

            return isNegative ? -absValue : absValue;
        }

        public static decimal Sqrt(this decimal value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
            decimal n = value / 2m + 1m;
            decimal n1 = (n + value / n) / 2m;

            while (n1 < n)
            {
                n = n1;
                n1 = (n + value / n) / 2m;
            }

            return n;
        }

        public static decimal Ceiling(this decimal value, decimal divisor)
        {
            return Math.Ceiling(value / divisor);
        }

        public static decimal Floor(this decimal value, decimal divisor)
        {
            return Math.Floor(value / divisor);
        }

        public static decimal Round(this decimal value)
        {
            return Math.Round(value);
        }

        public static float Sqrt(this float value)
        {
            return (float)Math.Sqrt(value);
        }

        public static float Ceiling(this float value, float divisor)
        {
            return (float)Math.Ceiling(value / divisor);
        }

        public static float Floor(this float value, float divisor)
        {
            return (float)Math.Floor(value / divisor);
        }

        public static float Round(this float value)
        {
            return (float)Math.Round(value);
        }

        public static int Ceiling(this int value, int divisor)
        {
            return (int)Math.Ceiling((double)value / divisor);
        }

        public static int Floor(this int value, int divisor)
        {
            return (int)Math.Floor((double)value / divisor);
        }

        public static int Round(this int value)
        {
            return (int)Math.Round((double)value);
        }

        public static long Sqrt(this long value)
        {
            return (long)Math.Sqrt(value);
        }

        public static long Ceiling(this long value, long divisor)
        {
            return (long)Math.Ceiling((double)value / divisor);
        }

        public static long Floor(this long value, long divisor)
        {
            return (long)Math.Floor((double)value / divisor);
        }

        public static long Round(this long value)
        {
            return (long)Math.Round((double)value);
        }
    }

}
