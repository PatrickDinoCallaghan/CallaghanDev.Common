using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
namespace CallaghanDev.Utilities.Extensions
{
    public static class FloatArrayExtensions
    {
        /// <summary>
        /// Normalizes an array of floats to the range [-1, 1].
        /// </summary>
        /// <param name="values">The array of float values to normalize.</param>
        /// <returns>A new array of normalized float values in the range [-1, 1].</returns>
        public static float[] NormalizeToRange(this float[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("The input array cannot be null or empty.", nameof(values));


            float AverageAvg = values.Average();

            // Calculate normalization factor

            var normalizationFactor = Math.Max(Math.Abs(values.Min()- AverageAvg), values.Max()- AverageAvg);
            normalizationFactor = Math.Max(normalizationFactor, 1);

            if (normalizationFactor == 0)
                return values.Select(_ => 0f).ToArray(); // Return all zeros if normalization factor is zero

            // Normalize values to range [-1, 1]
            return values.Select(v => (v- AverageAvg) / normalizationFactor).ToArray();
        }
        /// <summary>
        /// Normalizes an array of floats to the range [-1, 1] using a Gaussian-like transformation
        /// where 66% of the values fall within ±1 standard deviation of the mean.
        /// </summary>
        /// <param name="values">The array of float values to normalize.</param>
        /// <returns>A new array of normalized float values in the range [-1, 1].</returns>
        public static float[] NormalizeToGaussianRange(this float[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("The input array cannot be null or empty.", nameof(values));

            // Calculate the mean (average) of the values
            float mean = values.Average();

            // Calculate the standard deviation
            float stdDev = (float)Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));

            if (stdDev == 0)
                return values.Select(_ => 0f).ToArray(); // If standard deviation is zero, return all values as 0 (center of range)

            // Normalize values to the range [-1, 1] using the Gaussian formula
            return values.Select(v =>
            {
                // Transform to a Z-score
                float zScore = (v - mean) / stdDev;

                // Use the cumulative distribution function (CDF) of a standard normal distribution
                float cdf = 0.5f * (1 + (float)SpecialFunctions.Erf(zScore / (float)Math.Sqrt(2)));

                // Scale CDF to the range [-1, 1]
                float normalizedValue = 2 * cdf - 1;
                return normalizedValue;
            }).ToArray();
        }
    }
}
