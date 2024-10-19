using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming
{
    /// <summary>
    /// Class representing high, low, open, and close data
    /// </summary>
    public class HlocData
    {
        public decimal? High { get; set; }   // High price
        public decimal? Low { get; set; }    // Low price
        public decimal? Open { get; set; }   // Open price
        public decimal? Close { get; set; }  // Close price
    }
}
