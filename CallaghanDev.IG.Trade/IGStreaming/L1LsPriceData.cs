using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming
{
    /// <summary>
    /// Class representing level 1 price data
    /// </summary>
    public class L1LsPriceData
    {
        public decimal? MidOpen;     // Mid open price
        public decimal? High;        // High price
        public decimal? Low;         // Low price
        public decimal? Change;      // Change in price
        public decimal? ChangePct;   // Percentage change in price
        public string UpdateTime;    // Time of the update
        public int? MarketDelay;     // Delay in the market
        public string MarketState;   // State of the market
        public decimal? Bid;         // Bid price
        public decimal? Offer;       // Offer price
    }

}
