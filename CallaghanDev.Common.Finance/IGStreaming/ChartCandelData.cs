using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming
{
    /// <summary>
    /// Class representing chart candle data
    /// </summary>
    public class ChartCandelData : ChartDataBase
    {
        public HlocData Offer { get; set; }                // Offer price data
        public HlocData Bid { get; set; }                  // Bid price data
        public HlocData LastTradedPrice { get; set; }      // Last traded price data
        public bool? EndOfConsolidation { get; set; }      // Indicates end of consolidation
        public int? TickCount { get; set; }                // Tick count
    }
}
