using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Common.Finance.IGRest.Responses
{
    public class MarketDataResponse
    {
        public MarketSnapshot Snapshot { get; set; }
    }
    public class MarketSnapshot
    {
        public string MarketStatus { get; set; }
        public double NetChange { get; set; }
        public double PercentageChange { get; set; }
        public string UpdateTime { get; set; }
        public int DelayTime { get; set; }
        public double Bid { get; set; }
        public double Offer { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double? BinaryOdds { get; set; }
        public int DecimalPlacesFactor { get; set; }
        public int ScalingFactor { get; set; }
        public double ControlledRiskExtraSpread { get; set; }
    }
}
