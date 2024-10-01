using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming
{
    /// <summary>
    /// Class representing chart tick data
    /// </summary>
    public class ChartTickData : ChartDataBase
    {
        public decimal? Bid { get; set; }               // Bid price
        public decimal? Offer { get; set; }             // Offer price
        public decimal? LastTradedPrice { get; set; }   // Last traded price
    }
}
