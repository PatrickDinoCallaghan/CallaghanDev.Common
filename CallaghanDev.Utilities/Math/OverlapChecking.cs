using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.MathTools
{
    public static class OverlapChecking
    {
        /// <summary>
        /// Checks if two rectangles overlap, If they do, return true.
        /// </summary>
        /// <param name="Rec1"></param>
        /// <param name="Rec2"></param>
        /// <returns></returns>
        public static bool Clash(System.Drawing.Rectangle Rec1, System.Drawing.Rectangle Rec2)
        {
            // If horizontal or vertical overlaps are true, then the rectangles clash
            return XClash(Rec1, Rec2) || YClash(Rec1, Rec2);
        }
        public static bool XClash(System.Drawing.Rectangle Rec1, System.Drawing.Rectangle Rec2)
        {
            // Check for horizontal overlap
            return !(Rec1.Left >= Rec2.Right || Rec2.Left >= Rec1.Right);
        }
        public static bool YClash(System.Drawing.Rectangle Rec1, System.Drawing.Rectangle Rec2)
        {
            // Check for vertical overlap
            return !(Rec1.Top >= Rec2.Bottom || Rec2.Top >= Rec1.Bottom);
        }
        /// <summary>
        /// This is the clash check that all other clash checks are derived from. It checks if two periods of time clash
        /// </summary>
        /// <param name="Start1"></param>
        /// <param name="End1"></param>
        /// <param name="Start2"></param>
        /// <param name="End2"></param>
        /// <returns></returns>
        public static bool Clash(DateTime Start1, DateTime End1, DateTime Start2, DateTime End2)
        {
            if (!(Start1 >= End2 || Start2 >= End1))
            {
                return true;
            }
            return false;
        }
    }

}
