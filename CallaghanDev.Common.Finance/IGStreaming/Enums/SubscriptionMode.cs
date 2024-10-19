using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming.Enums
{
    public enum SubscriptionMode
    { 
        //MERGE; indicates that updates occurring very close together should only yield one update, and is used for account and price notifications to regulate the update rate) 
        [Description("MERGE")]
        Merge,
        [Description("DISTINCT")]
        //DISTINCT; indicates that each update should yield a notification and is required for trade notifications.
        Distinct
    }
}
