using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities
{
    public static class ArrayHelper
    {
        static string PrintArray<T>(T[] array)
        {
            if (array == null)
            {
                return "The provided array is null.";
            }
            string rtn = "";
            for (int i = 0; i < array.Length; i++)
            {
                rtn = rtn + $"int[{i}] = {array[i]},";
            }

            return rtn;
        }
        static long[] GenerateRandomArray(int size)
        {
            long[] result = new long[size];
            Random random = new Random();

            for (int i = 0; i < size; i++)
            {
                result[i] = ((long)random.Next()) << 32 | (long)random.Next();
            }

            return result;
        }
    }
}
