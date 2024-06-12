using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CallaghanDev.Utilities.String.StringTools;

namespace CallaghanDev.Utilities.String
{
    /// <summary>
    /// String Similarity index
    /// </summary>
    public class Similarity
    {
        JaroWinklerDistanceCalc _jwd;
        public Similarity()
        {
            _jwd = new JaroWinklerDistanceCalc();
        }
        public bool StringsAreVerySimilar(string InStr1, string InStr2)
        {

            if (System.Text.RegularExpressions.Regex.Replace(InStr1.ToLower(), @"\s+", "") == System.Text.RegularExpressions.Regex.Replace(InStr2.ToLower(), @"\s+", ""))
            {
                return true;
            }

            return false;
        }

        public bool StringsAreSimilar(string InStr1, string InStr2, double JWD = 0.15) // First actual good thing ive done in weeks
        {
            InStr1 = InStr1.Trim(); // Trim out any stupid whitespace. The user didnt mean to do this....
            InStr2 = InStr2.Trim();

            // First you need to check if an abbrivation has been used GSK is similar to glaxosmithkline
            char[] InStr1_CharArray = InStr1.ToCharArray();
            char[] InStr2_CharArray = InStr2.ToCharArray();

            string InStr1a_Abr = ""; string InStr2a_Abr = ""; // You are now comparing 4 strings moron, great idea, your CPU hates you.
            string InStr1b_Abr = ""; string InStr2b_Abr = ""; // 6 now, well done. Hope its worth it.

            //We do this so we dont keep comparing two non blank lists
            List<string> InStr1_List = new List<string>();    // This will be the non blank emtpy list of strings from InStr1
            List<string> InStr2_List = new List<string>();    // This will be the non blank emtpy list of strings from InStr2

            #region Loads Lists with possible abbreviations

            //Check all uppcase letters in a string.
            foreach (char item in InStr1_CharArray) // Makes an abbreviation from the first string input
            {
                if (char.IsUpper(item) == true)
                {
                    InStr1a_Abr = InStr1a_Abr + item;
                }
            }
            if (InStr1a_Abr != "")
            {
                InStr1_List.Add(InStr1a_Abr);
            }

            foreach (char item in InStr2_CharArray) // Makes an abbreviation from the second string input
            {
                if (char.IsUpper(item) == true)
                {
                    InStr2a_Abr = InStr2a_Abr + item;
                }
            }
            if (InStr1b_Abr != "")
            {
                InStr2_List.Add(InStr2a_Abr);
            }

            if (InStr1.Contains(" ") == true)
            {
                string[] Str_Arr_Temp1 = InStr1.Split(' ');
                foreach (string item in Str_Arr_Temp1)
                {
                    InStr1b_Abr = InStr1b_Abr + item.Substring(0, 1);
                }
            }
            if (InStr1b_Abr != "")
            {
                InStr1_List.Add(InStr1b_Abr);
            }

            if (InStr2.Contains(" ") == true)
            {
                string[] Str_Arr_Temp2 = InStr2.Split(' ');

                foreach (string item in Str_Arr_Temp2)
                {
                    InStr2b_Abr = InStr2b_Abr + item.Substring(0, 1);
                }
            }
            if (InStr2b_Abr != "")
            {
                InStr2_List.Add(InStr2b_Abr);
            }
            #endregion

            InStr1_List.Add(InStr1);
            InStr2_List.Add(InStr2);
            foreach (string item1 in InStr1_List)
            {
                foreach (string item2 in InStr2_List)
                {
                    if (item1.ToLower() == item2.ToLower()) // Keeping it static will improve performace
                    {
                        return true;
                    }
                }
            }

            if (_jwd.distance(InStr1, InStr2) < JWD) // Keeping it static will improve performace
            {
                return true;
            }

            return false;
        }

        public double JaroWinklerDistance(string aString1, string aString2)
        {
            return _jwd.distance(aString1, aString2);
        }

        public string StringMostSimilar(string aString1, List<string> ListOfStrings)
        {
            if (ListOfStrings.Count() == 0)
                return string.Empty;

            ListOfStrings.OrderByDescending(r => JaroWinklerDistance(aString1, r));
           
            return (string)ListOfStrings.FirstOrDefault();

        }

        private class JaroWinklerDistanceCalc
        {
            /* The Winkler modification will not be applied unless the 
             * percent match was at or above the mWeightThreshold percent 
             * without the modification. 
             * Winkler's paper used a default value of 0.7
             */
            private  readonly double mWeightThreshold = 0.7;

            /* Size of the prefix to be concidered by the Winkler modification. 
             * Winkler's paper used a default value of 4
             */
            private  readonly int mNumChars = 4;

            /// <summary>
            /// Returns the Jaro-Winkler distance between the specified  
            /// strings. The distance is symmetric and will fall in the 
            /// range 0 (perfect match) to 1 (no match). 
            /// </summary>
            /// <param name="aString1">First String</param>
            /// <param name="aString2">Second String</param>
            /// <returns></returns>
            public double distance(string aString1, string aString2)
            {
                return 1.0 - proximity(aString1, aString2);
            }

            /// <summary>
            /// Returns the Jaro-Winkler distance between the specified  
            /// strings. The distance is symmetric and will fall in the 
            /// range 0 (no match) to 1 (perfect match). 
            /// </summary>
            /// <param name="aString1">First String</param>
            /// <param name="aString2">Second String</param>
            /// <returns></returns>
            public double proximity(string aString1, string aString2)
            {
                if (string.IsNullOrEmpty(aString1))
                {
                    return 0;
                }
                int lLen1 = aString1.Length;
                int lLen2 = aString2.Length;
                if (lLen1 == 0)
                    return lLen2 == 0 ? 1.0 : 0.0;

                int lSearchRange = System.Math.Max(0, System.Math.Max(lLen1, lLen2) / 2 - 1);

                // default initialized to false
                bool[] lMatched1 = new bool[lLen1];
                bool[] lMatched2 = new bool[lLen2];

                int lNumCommon = 0;
                for (int i = 0; i < lLen1; ++i)
                {
                    int lStart = System.Math.Max(0, i - lSearchRange);
                    int lEnd = System.Math.Min(i + lSearchRange + 1, lLen2);
                    for (int j = lStart; j < lEnd; ++j)
                    {
                        if (lMatched2[j]) continue;
                        if (aString1[i] != aString2[j])
                            continue;
                        lMatched1[i] = true;
                        lMatched2[j] = true;
                        ++lNumCommon;
                        break;
                    }
                }
                if (lNumCommon == 0) return 0.0;

                int lNumHalfTransposed = 0;
                int k = 0;
                for (int i = 0; i < lLen1; ++i)
                {
                    if (!lMatched1[i]) continue;
                    while (!lMatched2[k]) ++k;
                    if (aString1[i] != aString2[k])
                        ++lNumHalfTransposed;
                    ++k;
                }
                // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
                int lNumTransposed = lNumHalfTransposed / 2;

                // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
                double lNumCommonD = lNumCommon;
                double lWeight = (lNumCommonD / lLen1
                                 + lNumCommonD / lLen2
                                 + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

                if (lWeight <= mWeightThreshold) return lWeight;
                int lMax = System.Math.Min(mNumChars, System.Math.Min(aString1.Length, aString2.Length));
                int lPos = 0;
                while (lPos < lMax && aString1[lPos] == aString2[lPos])
                    ++lPos;
                if (lPos == 0) return lWeight;
                return lWeight + 0.1 * lPos * (1.0 - lWeight);

            }

            public static double Similarities(string value, string aString1)
            {
                return new JaroWinklerDistanceCalc().distance(value, aString1);
            }

        }
        // Allows you to take the jarowiklerDistance of a string through an extension method

    }
}
