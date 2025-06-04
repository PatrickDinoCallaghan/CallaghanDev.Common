using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static CallaghanDev.Utilities.String.StringTools;

namespace CallaghanDev.Utilities.String
{
    /// <summary>
    /// Enumeration of supported string similarity algorithms
    ///
    /// JaroWinkler  Good for short strings prefix sensitive
    ///   Fast for short texts such as names
    ///   Accounts for character transpositions
    ///   Not suitable for long texts or documents
    ///
    /// Levenshtein  Edit distance insert delete replace
    ///   Measures true character level edits
    ///   Works well on typos and small mutations
    ///   Costly for long strings
    ///
    /// Jaccard  Set based similarity character level
    ///   Lightweight and intuitive
    ///   Good for quick comparisons
    ///   Ignores character order or duplicates
    ///
    /// DiceCoefficient  Bigram based comparison
    ///   Good balance of accuracy and performance
    ///   Sensitive to character order
    ///   Affected by string length skew
    ///
    /// LongestCommonSubsequence  Measures shared subsequences
    ///   Preserves order without requiring contiguity
    ///   Great for similarity over sequence structure
    ///   Expensive for very long strings
    ///
    /// Cosine  Vector based similarity using character frequency
    ///   Powerful for vectorized NLP inputs
    ///   Captures density and overlap
    ///   Not order sensitive may miss structure
    /// </summary>
    public enum StringSimilarityType
    {
        JaroWinkler,
        Levenshtein,
        Jaccard,
        DiceCoefficient,
        LongestCommonSubsequence,
        Cosine
    }

    public interface ISimilarityAlgorithm
    {
        public double Calculate(string aString1, string aString2);
    }

    /// <summary>
    /// String Similarity index
    /// </summary>
    public class Similarity : ISimilarityAlgorithm
    {
        private JaroWinklerSimilarity _jwd = new JaroWinklerSimilarity();
        private LevenshteinSimilarity _ldc = new LevenshteinSimilarity();
        private JaccardSimilarity _js = new JaccardSimilarity();
        private DiceCoefficientSimilarity _dcs = new DiceCoefficientSimilarity();
        private LongestCommonSubsequenceSimilarity _lcss = new LongestCommonSubsequenceSimilarity();
        private CosineSimilarity _cs = new CosineSimilarity();

        private ISimilarityAlgorithm similarityAlgorithm;

        public Similarity(StringSimilarityType stringSimilarityType = StringSimilarityType.Levenshtein)
        {
            similarityAlgorithm = stringSimilarityType switch
            {
                StringSimilarityType.JaroWinkler => _jwd,
                StringSimilarityType.Levenshtein => _ldc,
                StringSimilarityType.Jaccard => _js,
                StringSimilarityType.DiceCoefficient => _dcs,
                StringSimilarityType.LongestCommonSubsequence => _lcss,
                StringSimilarityType.Cosine => _cs,
                _ => throw new ArgumentOutOfRangeException(nameof(stringSimilarityType), "Unsupported similarity type")
            };
        }



        public double Calculate(string aString1, string aString2)
        {
            return similarityAlgorithm.Calculate(aString1, aString2);
        }
        public string StringMostSimilar(string aString1, List<string> ListOfStrings)
        {
            if (ListOfStrings.Count == 0)
                return string.Empty;

            var MostSimilarString = ListOfStrings
                .OrderByDescending(r => similarityAlgorithm.Calculate(aString1, r))
                .FirstOrDefault();

            return MostSimilarString;
        }

        public class JaroWinklerSimilarity : ISimilarityAlgorithm
        {
            private readonly double threshold = 0.7;
            private readonly int prefixSize = 4;

            public double Calculate(string s1, string s2)
            {
                if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;
                if (s1 == s2) return 1.0;

                int len1 = s1.Length, len2 = s2.Length;
                int matchDistance = Math.Max(len1, len2) / 2 - 1;

                bool[] s1Matches = new bool[len1];
                bool[] s2Matches = new bool[len2];

                int matches = 0, transpositions = 0;

                for (int i = 0; i < len1; i++)
                {
                    int start = Math.Max(0, i - matchDistance);
                    int end = Math.Min(i + matchDistance + 1, len2);

                    for (int j = start; j < end; j++)
                    {
                        if (s2Matches[j]) continue;
                        if (s1[i] != s2[j]) continue;
                        s1Matches[i] = s2Matches[j] = true;
                        matches++;
                        break;
                    }
                }

                if (matches == 0) return 0.0;

                for (int i = 0, k = 0; i < len1; i++)
                {
                    if (!s1Matches[i]) continue;
                    while (!s2Matches[k]) k++;
                    if (s1[i] != s2[k]) transpositions++;
                    k++;
                }

                double m = matches;
                double jaro = (m / len1 + m / len2 + (m - transpositions / 2.0) / m) / 3.0;

                int prefix = 0;
                for (int i = 0; i < Math.Min(prefixSize, Math.Min(len1, len2)); i++)
                {
                    if (s1[i] == s2[i]) prefix++;
                    else break;
                }

                return jaro < threshold ? jaro : jaro + 0.1 * prefix * (1 - jaro);
            }
        }

        public class LevenshteinSimilarity : ISimilarityAlgorithm
        {
            public double Calculate(string s1, string s2)
            {
                int[,] dp = new int[s1.Length + 1, s2.Length + 1];
                for (int i = 0; i <= s1.Length; i++) dp[i, 0] = i;
                for (int j = 0; j <= s2.Length; j++) dp[0, j] = j;

                for (int i = 1; i <= s1.Length; i++)
                {
                    for (int j = 1; j <= s2.Length; j++)
                    {
                        int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                        dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                    }
                }

                return 1.0 - (double)dp[s1.Length, s2.Length] / Math.Max(s1.Length, s2.Length);
            }
        }

        public class JaccardSimilarity : ISimilarityAlgorithm
        {
            public double Calculate(string s1, string s2)
            {
                var set1 = new HashSet<char>(s1);
                var set2 = new HashSet<char>(s2);

                var intersection = new HashSet<char>(set1);
                intersection.IntersectWith(set2);

                var union = new HashSet<char>(set1);
                union.UnionWith(set2);

                return union.Count == 0 ? 0.0 : (double)intersection.Count / union.Count;
            }
        }

        public class DiceCoefficientSimilarity : ISimilarityAlgorithm
        {
            public double Calculate(string s1, string s2)
            {
                var bigrams1 = GetBigrams(s1);
                var bigrams2 = GetBigrams(s2);

                int intersection = bigrams1.Intersect(bigrams2).Count();
                return (2.0 * intersection) / (bigrams1.Count + bigrams2.Count);
            }

            private List<string> GetBigrams(string input)
            {
                var list = new List<string>();
                for (int i = 0; i < input.Length - 1; i++)
                    list.Add(input.Substring(i, 2));
                return list;
            }
        }

        public class LongestCommonSubsequenceSimilarity : ISimilarityAlgorithm
        {
            public double Calculate(string s1, string s2)
            {
                int[,] dp = new int[s1.Length + 1, s2.Length + 1];
                for (int i = 1; i <= s1.Length; i++)
                {
                    for (int j = 1; j <= s2.Length; j++)
                    {
                        if (s1[i - 1] == s2[j - 1])
                            dp[i, j] = dp[i - 1, j - 1] + 1;
                        else
                            dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
                return (double)dp[s1.Length, s2.Length] / Math.Max(s1.Length, s2.Length);
            }
        }

        public class CosineSimilarity : ISimilarityAlgorithm
        {
            public double Calculate(string s1, string s2)
            {
                var vec1 = GetCharFreq(s1);
                var vec2 = GetCharFreq(s2);

                var allKeys = vec1.Keys.Union(vec2.Keys);
                double dotProduct = allKeys.Sum(k => vec1.GetValueOrDefault(k) * vec2.GetValueOrDefault(k));
                double magnitude1 = Math.Sqrt(vec1.Values.Sum(v => v * v));
                double magnitude2 = Math.Sqrt(vec2.Values.Sum(v => v * v));

                return (magnitude1 * magnitude2) == 0 ? 0.0 : dotProduct / (magnitude1 * magnitude2);
            }

            private Dictionary<char, int> GetCharFreq(string input)
            {
                return input.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            }
        }
    }

}