using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Common.Math
{
    public static class MathExtensions
    {
        public static int GreatestCommonDivisor( int left, int right)
        {
            // Ensure the numbers are non-negative
            left = System.Math.Abs(left);
            right = System.Math.Abs(right);

            // Base case: if one of the numbers is zero, return the other number
            if (right == 0)
                return left;
            if (left == 0)
                return right;

            // Recursive case: apply the Euclidean algorithm
            return GreatestCommonDivisor(right, left % right);
        }
    }
}
