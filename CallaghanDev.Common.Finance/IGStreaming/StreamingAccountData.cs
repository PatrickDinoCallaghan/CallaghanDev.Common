using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming
{
    /// <summary>
    /// Class representing streaming account data
    /// </summary>
    public class StreamingAccountData
    {
        public decimal? ProfitAndLoss; // Profit and loss
        public decimal? Deposit;       // Deposit amount
        public decimal? UsedMargin;    // Used margin
        public decimal? AmountDue;     // Amount due
        public decimal? AvailableCash; // Available cash
    }

}
