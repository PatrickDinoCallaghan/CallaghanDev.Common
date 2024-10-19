using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming.Enums
{
    /// <summary>
    /// Enum representing the type of trade subscription
    /// </summary>
    public enum TradeSubscriptionTypeEnum
    {
        WOU = 0,  // Working order update
        OPU = 1,  // Open position update
        TRADE = 2 // Trade update
    }

}
