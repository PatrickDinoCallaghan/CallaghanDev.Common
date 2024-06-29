using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Math
{
    public class BernoulliNumbers
    {
        public double CalculateBn(int n)
        {
            double[] A = new double[n + 1];

            for (int m = 0; m <= n; m++)
            {
                A[m] = 1.0 / (m + 1);
                for (int j = m; j >= 1; j--)
                {
                    A[j - 1] = j * (A[j - 1] - A[j]);
                }
            }

            return A[0]; // Bn
        }
    }
}
