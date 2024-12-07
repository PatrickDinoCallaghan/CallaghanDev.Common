using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Common.Math
{
    public static class PowerExtensions
    {
        public static decimal Pow(this decimal baseValue, int exponent)
        {
            if (exponent == 0) return 1; // Any number to the power of 0 is 1
            if (exponent < 0) return 1 / Pow(baseValue, -exponent); // Handle negative exponents

            decimal result = 1;
            decimal currentBase = baseValue;

            while (exponent > 0)
            {
                if ((exponent & 1) == 1) // If exponent is odd
                {
                    result *= currentBase;
                }

                currentBase *= currentBase; // Square the base
                exponent >>= 1; // Divide exponent by 2
            }

            return result;
        }
        public static double Pow(this double baseValue, int exponent)
        {
            return System.Math.Pow(baseValue, exponent);
        }
    }
}
