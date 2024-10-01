using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming
{

    /// <summary>
    /// Class representing update arguments
    /// </summary>
    public class UpdateArgs<T> : EventArgs
    {
        public T UpdateData { get; set; }     // Update data of generic type T
        public int ItemPosition { get; set; } // Item position
        public string ItemName { get; set; }  // Item name
    }
}
