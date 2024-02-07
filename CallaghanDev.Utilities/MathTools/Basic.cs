using System.Diagnostics;

namespace CallaghanDev.Utilities.MathTools
{
    public static class Basic
    {
        public static int AbsoluteValue(this int inint)
        {
            return inint * inint ^ 1 / 2;
        }
        public static TimeSpan AbsoluteValue(this TimeSpan InSpan)
        {
            TimeSpan BlankTimespan = new TimeSpan();

            if (InSpan < BlankTimespan)
            {
                return -InSpan;
            }
            else
            {
                return InSpan;
            }
        }
        public static bool WithinRange(double Value, double Min, double Max)
        {
            if ((Value - Min) * (Max - Value) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool CheckValueIsWithinRangeOfValue(double Value, double checkvalue, double tolerance)
        {
            if (tolerance == 1)
            {
                return true;
            }
            double MinMax = System.Math.Abs(Value * tolerance);

            return WithinRange(checkvalue, Value - MinMax, Value + MinMax);

        }
    }
}
