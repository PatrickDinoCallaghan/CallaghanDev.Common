using System.Diagnostics;
using System.Numerics;

namespace CallaghanDev.Utilities.MathTools
{
    public static class Basic
    {
        public static int AbsoluteValue(this int inint)
        {
            return inint * inint ^ 1 / 2;
        }
        public static TimeSpan AbsoluteValue(this TimeSpan InSpan)
        {
            TimeSpan BlankTimespan = new TimeSpan();

            if (InSpan < BlankTimespan)
            {
                return -InSpan;
            }
            else
            {
                return InSpan;
            }
        }
        public static bool WithinRange(double Value, double Min, double Max)
        {
            if ((Value - Min) * (Max - Value) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool CheckValueIsWithinRangeOfValue(double Value, double checkvalue, double tolerance)
        {
            if (tolerance == 1)
            {
                return true;
            }
            double MinMax = System.Math.Abs(Value * tolerance);

            return WithinRange(checkvalue, Value - MinMax, Value + MinMax);

        }

        public static float Random(float min, float max)
        {
            return (float)(new System.Random().NextDouble() * (max - min) + min);
        }
        public static decimal Random(decimal min, decimal max)
        {
            var random = new System.Random();
            decimal range = max - min;
            var scale = (decimal)random.NextDouble();
            return min + (scale * range);
        }
        public static int Random(int min, int max)
        {
            return new System.Random().Next(min, max + 1);
        }
        public static long Random(long min, long max)
        {
            byte[] buf = new byte[8];
            new System.Random().NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
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
        } // Extension method for Floor division on BigInteger
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
    }
}
