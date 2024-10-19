using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGRest
{
    public class Market
    {
        public string epic { get; set; }
        public string instrumentName { get; set; }
        public string instrumentType { get; set; }
        public string expiry { get; set; }
        public double? high { get; set; }
        public double? low { get; set; }
        public double? percentageChange { get; set; }
        public double? netChange { get; set; }
        public string updateTime { get; set; }
        public string updateTimeUTC { get; set; }
        public double? bid { get; set; }
        public double? offer { get; set; }
        public int delayTime { get; set; }
        public bool streamingPricesAvailable { get; set; }
        public string marketStatus { get; set; }
        public int scalingFactor { get; set; }
        public override string ToString()
        {
            return $"Market Details:\n" +
                   $"Epic: {epic}\n" +
                   $"Instrument Name: {instrumentName}\n" +
                   $"Instrument Type: {instrumentType}\n" +
                   $"Expiry: {expiry}\n" +
                   $"High: {high}\n" +
                   $"Low: {low}\n" +
                   $"Percentage Change: {percentageChange}\n" +
                   $"Net Change: {netChange}\n" +
                   $"Update Time: {updateTime}\n" +
                   $"Update Time UTC: {updateTimeUTC}\n" +
                   $"Bid: {bid}\n" +
                   $"Offer: {offer}\n" +
                   $"Delay Time: {delayTime}\n" +
                   $"Streaming Prices Available: {streamingPricesAvailable}\n" +
                   $"Market Status: {marketStatus}\n" +
                   $"Scaling Factor: {scalingFactor}";
        }
    }
}
