using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Common.Math
{
    public class QuantileNormalizer
    {
        /// <summary>
        /// Given two arrays of integers (source and target) and a value from the source array,
        /// predicts the corresponding value in the target array using quantile normalization.
        /// </summary>
        /// <param name="source">The source dataset (e.g. from liquidity provider A).</param>
        /// <param name="target">The target dataset (e.g. from liquidity provider B).</param>
        /// <param name="value">A value from the source dataset for which you want a prediction.</param>
        /// <returns>The predicted value from the target dataset corresponding to the input value.</returns>
        public double PredictValueFromSourceToTarget(int[] source, int[] target, int value)
        {
            if (source == null || target == null)
                throw new ArgumentNullException("Input arrays cannot be null.");
            if (source.Length == 0 || target.Length == 0)
                throw new ArgumentException("Input arrays must contain at least one element.");

            // Sort both arrays so we can compute quantiles.
            int[] sortedSource = source.OrderBy(x => x).ToArray();
            int[] sortedTarget = target.OrderBy(x => x).ToArray();

            // Compute the quantile rank of 'value' in the source array.
            double quantile = GetQuantile(sortedSource, value);

            // Use that quantile to predict the corresponding value in the target array.
            double predictedValue = InterpolateFromQuantile(sortedTarget, quantile);
            return predictedValue;
        }

        /// <summary>
        /// Computes the quantile rank of a given value in a sorted array.
        /// It uses the midpoint method: (number of elements less than the value + 0.5 * number equal) / total elements.
        /// </summary>
        private double GetQuantile(int[] sortedData, int value)
        {
            int n = sortedData.Length;
            int countLess = 0;
            int countEqual = 0;
            foreach (int x in sortedData)
            {
                if (x < value)
                    countLess++;
                else if (x == value)
                    countEqual++;
            }
            double midRank = countLess + 0.5 * countEqual;
            return midRank / n;  // quantile in [0, 1]
        }

        /// <summary>
        /// Given a sorted array and a quantile (between 0 and 1), this method uses linear interpolation 
        /// to return the corresponding value in the array.
        /// </summary>
        private double InterpolateFromQuantile(int[] sortedData, double quantile)
        {
            int n = sortedData.Length;
            // Scale quantile to the index range [0, n-1]
            double position = quantile * (n - 1);
            int indexLower = (int)System.Math.Floor(position);
            int indexUpper = (int)System.Math.Ceiling(position);

            // If position is an integer, return that element directly.
            if (indexLower == indexUpper)
                return sortedData[indexLower];

            // Otherwise, linearly interpolate between the two adjacent elements.
            double weightUpper = position - indexLower;
            double weightLower = 1.0 - weightUpper;
            return sortedData[indexLower] * weightLower + sortedData[indexUpper] * weightUpper;
        }
    }

}
