using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming
{
    /// <summary>
    /// Base class for chart data
    /// </summary>
    public class ChartDataBase
    {
        public decimal? LastTradedVolume { get; set; }        // Last traded volume
        public decimal? IncrimetalTradingVolume { get; set; } // Incremental trading volume
        public DateTime? UpdateTime { get; set; }             // Update time
        public decimal? DayMidOpenPrice { get; set; }         // Mid open price for the day
        public decimal? DayChange { get; set; }               // Change from open price to current (MID price)
        public decimal? DayChangePct { get; set; }            // Daily percentage change (MID price)
        public decimal? DayHigh { get; set; }                 // Daily high price (MID)
        public decimal? DayLow { get; set; }                  // Daily low price (MID)
    }
}
