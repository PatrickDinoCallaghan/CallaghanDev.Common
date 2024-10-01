using Lightstreamer.DotNet.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming.Listerners
{    /// <summary>
     /// Listener class for chart tick table
     /// </summary>
    public class ChartTickTableListerner : TableListenerAdapterBase<ChartTickData>
    {
        /// <summary>
        /// Loads update data into a ChartTickData object
        /// </summary>
        protected override ChartTickData LoadUpdate(IUpdateInfo update)
        {
            try
            {
                var updateData = new ChartTickData
                {
                    Bid = StringToNullableDecimal(update.GetNewValue("BID")),
                    Offer = StringToNullableDecimal(update.GetNewValue("OFR")),
                    LastTradedPrice = StringToNullableDecimal(update.GetNewValue("LTP")),
                    LastTradedVolume = StringToNullableDecimal(update.GetNewValue("LTV")),
                    IncrimetalTradingVolume = StringToNullableDecimal(update.GetNewValue("TTV")),
                    UpdateTime = EpocStringToNullableDateTime(update.GetNewValue("UTM")),
                    DayMidOpenPrice = StringToNullableDecimal(update.GetNewValue("DAY_OPEN_MID")),
                    DayChange = StringToNullableDecimal(update.GetNewValue("DAY_NET_CHG_MID")),
                    DayChangePct = StringToNullableDecimal(update.GetNewValue("DAY_PERC_CHG_MID")),
                    DayHigh = StringToNullableDecimal(update.GetNewValue("DAY_HIGH")),
                    DayLow = StringToNullableDecimal(update.GetNewValue("DAY_LOW"))
                };
                return updateData; // Return the populated object
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Exception in LoadUpdate (ChartTick): {ex.Message}");
                return null; // Return null if an exception occurs
            }
        }
    }
}
