using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming.Enums
{

    /// <summary>
    /// Enum representing the status of a streaming trade
    /// </summary>
    public enum StreamingStatusEnum
    {
        OPEN,    // Trade is open
        UPDATED, // Trade has been updated
        AMENDED, // Trade has been amended
        CLOSED,  // Trade is closed
        DELETED, // Trade is deleted
    }
}
