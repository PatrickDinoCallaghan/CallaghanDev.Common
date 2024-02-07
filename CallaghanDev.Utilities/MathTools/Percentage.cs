using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.MathTools
{
    public static class Percentage
    {
        public static double Get<T>(T value, T total) where T : IConvertible
        {
            // Convert values to double
            double doubleValue = Convert.ToDouble(value);
            double doubleTotal = Convert.ToDouble(total);

            if (doubleTotal == 0)
            {
                throw new ArgumentException("Total cannot be zero when calculating a percentage.");
            }

            double percentage = doubleValue / doubleTotal * 100;

            // Return formatted value
            return System.Math.Round(percentage, 2);
        }
        public static double Get<T1, T2>(this T1 value, T2 total)
        where T1 : IConvertible
        where T2 : IConvertible
        {
            // Convert values to double
            double doubleValue = Convert.ToDouble(value);
            double doubleTotal = Convert.ToDouble(total);

            if (doubleTotal == 0)
            {
                throw new ArgumentException("Total cannot be zero when calculating a percentage.");
            }

            double percentage = doubleValue / doubleTotal * 100;

            // Return formatted value
            return System.Math.Round(percentage, 2);
        }

    }

}
