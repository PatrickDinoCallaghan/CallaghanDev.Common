using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Extensions
{
    public static class ParallelExtensions
    {
        public static void For(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int> body)
        {
            Parallel.For(fromInclusive, toExclusive, parallelOptions, body);
        }
    }
}
