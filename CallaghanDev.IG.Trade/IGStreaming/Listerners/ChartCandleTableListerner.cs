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
    /// Listener class for chart candle table
    /// </summary>
    public class ChartCandleTableListerner : TableListenerAdapterBase<ChartCandelData>
    {
        /// <summary>
        /// Loads update data into a ChartCandelData object
        /// </summary>
        protected override ChartCandelData LoadUpdate(IUpdateInfo update)
        {
            try
            {
                var updateData = new ChartCandelData
                {
                    Bid = new HlocData
                    {
                        High = StringToNullableDecimal(update.GetNewValue("BID_HIGH")),
                        Low = StringToNullableDecimal(update.GetNewValue("BID_LOW")),
                        Open = StringToNullableDecimal(update.GetNewValue("BID_OPEN")),
                        Close = StringToNullableDecimal(update.GetNewValue("BID_CLOSE"))
                    },
                    Offer = new HlocData
                    {
                        High = StringToNullableDecimal(update.GetNewValue("OFR_HIGH")),
                        Low = StringToNullableDecimal(update.GetNewValue("OFR_LOW")),
                        Open = StringToNullableDecimal(update.GetNewValue("OFR_OPEN")),
                        Close = StringToNullableDecimal(update.GetNewValue("OFR_CLOSE"))
                    },
                    LastTradedPrice = new HlocData
                    {
                        High = StringToNullableDecimal(update.GetNewValue("LTP_HIGH")),
                        Low = StringToNullableDecimal(update.GetNewValue("LTP_LOW")),
                        Open = StringToNullableDecimal(update.GetNewValue("LTP_OPEN")),
                        Close = StringToNullableDecimal(update.GetNewValue("LTP_CLOSE"))
                    },
                    LastTradedVolume = StringToNullableDecimal(update.GetNewValue("LTV")),
                    IncrimetalTradingVolume = StringToNullableDecimal(update.GetNewValue("TTV")),
                    UpdateTime = EpocStringToNullableDateTime(update.GetNewValue("UTM")),
                    DayMidOpenPrice = StringToNullableDecimal(update.GetNewValue("DAY_OPEN_MID")),
                    DayChange = StringToNullableDecimal(update.GetNewValue("DAY_NET_CHG_MID")),
                    DayChangePct = StringToNullableDecimal(update.GetNewValue("DAY_PERC_CHG_MID")),
                    DayHigh = StringToNullableDecimal(update.GetNewValue("DAY_HIGH")),
                    DayLow = StringToNullableDecimal(update.GetNewValue("DAY_LOW")),
                    TickCount = StringToNullableInt(update.GetNewValue("CONS_TICK_COUNT"))
                };
                // Check if end of consolidation flag is present
                var conEnd = StringToNullableInt(update.GetNewValue("CONS_END"));
                updateData.EndOfConsolidation = conEnd.HasValue ? conEnd > 0 : null;
                return updateData; // Return the populated object
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Exception in LoadUpdate (ChartCandle): {ex.Message}");
                return null; // Return null if an exception occurs
            }
        }
    }

}
