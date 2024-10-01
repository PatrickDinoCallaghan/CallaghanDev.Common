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
    /// Listener class for account details table
    /// </summary>
    public class AccountDetailsTableListerner : TableListenerAdapterBase<StreamingAccountData>
    {
        /// <summary>
        /// Loads update data into a StreamingAccountData object
        /// </summary>
        protected override StreamingAccountData LoadUpdate(IUpdateInfo update)
        {
            try
            {
                var streamingAccountData = new StreamingAccountData
                {
                    ProfitAndLoss = StringToNullableDecimal(update.GetNewValue("PNL")),
                    Deposit = StringToNullableDecimal(update.GetNewValue("DEPOSIT")),
                    UsedMargin = StringToNullableDecimal(update.GetNewValue("USED_MARGIN")),
                    AmountDue = StringToNullableDecimal(update.GetNewValue("AMOUNT_DUE")),
                    AvailableCash = StringToNullableDecimal(update.GetNewValue("AVAILABLE_CASH"))
                };
                return streamingAccountData; // Return the populated object
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Exception in LoadUpdate (AccountDetails): {ex.Message}");
                return null; // Return null if an exception occurs
            }
        }
    }
}
