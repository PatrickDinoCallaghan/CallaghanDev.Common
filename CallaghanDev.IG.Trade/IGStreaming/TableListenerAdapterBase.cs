using Lightstreamer.DotNet.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.IG.Trade.IGStreaming
{    
    /// <summary>
     /// Generic base class for table listener adapters
     /// </summary>
    public abstract class TableListenerAdapterBase<T> : TableListenerAdapterBase
    {
        /// <summary>
        /// Overrides the OnUpdate method to process updates
        /// </summary>
        public override void OnUpdate(int itemPos, string itemName, IUpdateInfo update)
        {
            // Load the update data
            var updateData = LoadUpdate(update);
            // Raise the Update event with the loaded data
            OnUpdate(new UpdateArgs<T> { UpdateData = updateData, ItemPosition = itemPos, ItemName = itemName });
        }

        /// <summary>
        /// Abstract method to load update data from IUpdateInfo
        /// </summary>
        protected abstract T LoadUpdate(IUpdateInfo update);

        /// <summary>
        /// Event handler for updates
        /// </summary>
        public event EventHandler<UpdateArgs<T>> Update;

        /// <summary>
        /// Raises the Update event
        /// </summary>
        protected virtual void OnUpdate(UpdateArgs<T> e)
        {
            Update?.Invoke(this, e); // Invoke the event if there are subscribers
        }
    }
    /// <summary>
    /// Base class for table listener adapters
    /// </summary>
    public class TableListenerAdapterBase : IHandyTableListener
    {
        /// <summary>
        /// Called when raw updates are lost
        /// </summary>
        public virtual void OnRawUpdatesLost(int itemPos, string itemName, int lostUpdates)
        {
            // Log the lost updates
            Debug.WriteLine($"Raw updates lost for item {itemName} at position {itemPos}. Lost updates: {lostUpdates}");
        }

        /// <summary>
        /// Called when a snapshot ends
        /// </summary>
        public virtual void OnSnapshotEnd(int itemPos, string itemName)
        {
            // Log the snapshot end
            Debug.WriteLine($"Snapshot end for item {itemName} at position {itemPos}.");
        }

        /// <summary>
        /// Called when an item is unsubscribed
        /// </summary>
        public virtual void OnUnsubscr(int itemPos, string itemName)
        {
            // Log the item unsubscription
            Debug.WriteLine($"Unsubscribed from item {itemName} at position {itemPos}.");
        }

        /// <summary>
        /// Called when all items are unsubscribed
        /// </summary>
        public virtual void OnUnsubscrAll()
        {
            // Log the unsubscription of all items
            Debug.WriteLine("Unsubscribed from all items.");
        }

        /// <summary>
        /// Called when an update is received
        /// </summary>
        public virtual void OnUpdate(int itemPos, string itemName, IUpdateInfo update)
        {
            // Implement logic for handling updates if needed
            Debug.WriteLine($"Update received for item {itemName} at position {itemPos}.");
        }

        /// <summary>
        /// Converts a string to a nullable decimal
        /// </summary>
        protected decimal? StringToNullableDecimal(string value)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal number))
            {
                return number;
            }
            return null; // Return null if parsing fails
        }

        /// <summary>
        /// Converts a string to a nullable integer
        /// </summary>
        protected int? StringToNullableInt(string value)
        {
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int number))
            {
                return number;
            }
            return null; // Return null if parsing fails
        }

        /// <summary>
        /// Converts an epoch time string to a nullable DateTime
        /// </summary>
        protected DateTime? EpocStringToNullableDateTime(string value)
        {
            if (ulong.TryParse(value, out ulong epoc))
            {
                // Add milliseconds to the Unix epoch start date
                return new DateTime(1970, 1, 1).AddMilliseconds(epoc);
            }
            return null; // Return null if parsing fails
        }
    }
}
