using Lightstreamer.DotNet.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming.Listerners
{
    /// <summary>
    /// Listener class for market details table
    /// </summary>
    public class MarketDetailsTableListerner : TableListenerAdapterBase<L1LsPriceData>
    {
        /// <summary>
        /// Loads update data into an L1LsPriceData object
        /// </summary>
        protected override L1LsPriceData LoadUpdate(IUpdateInfo update)
        {
            try
            {
                var lsL1PriceData = new L1LsPriceData
                {
                    MidOpen = StringToNullableDecimal(update.GetNewValue("MID_OPEN")),
                    High = StringToNullableDecimal(update.GetNewValue("HIGH")),
                    Low = StringToNullableDecimal(update.GetNewValue("LOW")),
                    Change = StringToNullableDecimal(update.GetNewValue("CHANGE")),
                    ChangePct = StringToNullableDecimal(update.GetNewValue("CHANGE_PCT")),
                    UpdateTime = update.GetNewValue("UPDATE_TIME"),
                    MarketDelay = StringToNullableInt(update.GetNewValue("MARKET_DELAY")),
                    MarketState = update.GetNewValue("MARKET_STATE"),
                    Bid = StringToNullableDecimal(update.GetNewValue("BID")),
                    Offer = StringToNullableDecimal(update.GetNewValue("OFFER"))
                };
                return lsL1PriceData; // Return the populated object
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Exception in LoadUpdate (MarketDetails): {ex.Message}");
                return null; // Return null if an exception occurs
            }
        }
    }
}
