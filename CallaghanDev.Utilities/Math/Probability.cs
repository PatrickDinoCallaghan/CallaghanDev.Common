using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.MathTools
{
    public static class Probability
    {
        public static class Odds
        {
            public static decimal Probability(string Fraction)
            {
                string[] InOdds_arr = Fraction.Split('/');

                if (Fraction.Contains('/'))
                {
                    if (InOdds_arr.Length == 2)
                    {
                        if (DataValidation.IsNumeric(InOdds_arr[0]) &&
                           DataValidation.IsNumeric(InOdds_arr[1]))
                        {
                            return Probability(Convert.ToDecimal(InOdds_arr[0]), Convert.ToDecimal(InOdds_arr[1]));
                        }
                    }
                }
                throw new ArgumentException("Fractional odds not given in the right format.");
            }
            public static decimal Probability(decimal InNumerator, decimal InDenominator)
            {
                return 1 - InNumerator / (InNumerator + InDenominator);
            }

            public enum Format
            {
                unknown,
                Fractional,
                EuropeanDecimal,
                MoneyLine,
            }
            public static double Combinations(int n, int k)
            {
                return Factorial(n) / (Factorial(k) * Factorial(n - k));
            }

            public static double Factorial(int n)
            {
                double result = 1;
                for (int i = 2; i <= n; i++)
                {
                    result *= i;
                }
                return result;
            }

            public static Format GetFormat(string InOdds)
            {
                if (DataValidation.IsNumeric(InOdds))
                {
                    return Format.Fractional;
                }
                else if (InOdds.Contains('/'))
                {
                    string[] OddsArr = InOdds.Split('/');
                    if (OddsArr.Length == 2 && InOdds.Length > 2)
                    {
                        if (OddsArr[0].Length > 0 && OddsArr[1].Length > 0)
                        {
                            if (DataValidation.IsNumeric(OddsArr[0]) && DataValidation.IsNumeric(OddsArr[1]))
                            {
                                return Format.Fractional;
                            }
                        }
                    }
                }

                return Format.unknown;
            }
        }

    }
}
