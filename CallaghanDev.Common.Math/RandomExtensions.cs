using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Common.Math
{
    public static class RandomExtensions
    {
        public static float Get(this Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }
        public static decimal Get(this Random random, decimal min, decimal max)
        {
            decimal range = max - min;
            var scale = (decimal)random.NextDouble();
            return min + (scale * range);
        }
        public static int Get(this Random random, int min, int max)
        {
            return random.Next(min, max + 1);
        }
        public static long Get(this Random random, long min, long max)
        {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return ((longRand % (max - min)) + min).AbsoluteValue();
        }
    }
}
