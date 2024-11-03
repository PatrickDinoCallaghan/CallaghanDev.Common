using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Lightstreamer.DotNet.Client;
using CallaghanDev.IG.Trade.IGStreaming.Enums;
using System.Net.Http.Headers;
using CallaghanDev.IG.Trade.IGStreaming.Listerners;
using System.ComponentModel;
using CallaghanDev.IG.Trade.Extensions;

namespace CallaghanDev.IG
{
    /// <summary>
    /// Client class for IG Streaming API
    /// </summary>
    public partial class TradingAPI : IConnectionListener
    {
        private LSClient lsClient;              // Lightstreamer client instance
        private long totalBytesReceived = 0;    // Field to track total bytes received

        /// <summary>
        /// Connects to the Lightstreamer server
        /// </summary>
        public bool ConnectStreaming()
        {
            bool connectionEstablished = false;                     // Flag to indicate if connection is established

            var connectionInfo = new ConnectionInfo();              // Create new ConnectionInfo object
            connectionInfo.Adapter = "DEFAULT";                     // Set the adapter name
            connectionInfo.User = identifier;                       // Set the username
            connectionInfo.Password = string.Format("CST-{0}|XST-{1}", cstToken, xSecurityToken); // Set the password
            connectionInfo.PushServerUrl = lsHost;                  // Set the server URL

            try
            {
                if (lsClient != null)
                {
                    lsClient.OpenConnection(connectionInfo, this);  // Open the connection
                    connectionEstablished = true;                   // Set the flag to true
                    Debug.WriteLine("Connection established successfully.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Exception in Connect: {ex.Message}");
                connectionEstablished = false;                      // Set the flag to false
            }
            return connectionEstablished;                           // Return the connection status
        }

        /// <summary>
        /// Disconnects from the Lightstreamer server
        /// </summary>
        public void DisconnectStreaming()
        {
            if (lsClient != null)
            {
                lsClient.CloseConnection(); // Close the connection
                Debug.WriteLine("Disconnected from Lightstreamer server.");
            }
        }

        /// <summary>
        /// Obsolete method for disconnecting
        /// </summary>
        [Obsolete("Use 'Disconnect' instead, this will be removed in future versions")]
        public void disconnect()
        {
            DisconnectStreaming(); // Call the new Disconnect method
        }

        #region Subscription methods

        /// <summary>
        /// Subscribes to account details updates
        /// </summary>
        public SubscribedTableKey SubscribeToAccountDetails(string accountId, IHandyTableListener tableListener)
        {
            // Default fields to subscribe to
            var fields = new[] { "PNL", "DEPOSIT", "USED_MARGIN", "AMOUNT_DUE", "AVAILABLE_CASH" };
            return SubscribeToAccountDetails(accountId, tableListener, fields); // Call the overloaded method
        }

        /// <summary>
        /// Subscribes to account details updates with specified fields
        /// </summary>
        public SubscribedTableKey SubscribeToAccountDetails(string accountId, IHandyTableListener tableListener, IEnumerable<string> fields)
        {
            var extTableInfo = new ExtendedTableInfo(
                new[] { "ACCOUNT:" + accountId }, // Items to subscribe to
                "MERGE",                          // Mode of subscription
                fields.ToArray(),                 // Fields to subscribe to
                true                              // Snapshot required
            );
            return lsClient.SubscribeTable(extTableInfo, tableListener, false); // Subscribe to the table
        }


        /// <summary>
        /// Subscribes to market details updates
        /// </summary>
        public SubscribedTableKey SubscribeToMarketDetails(string epic, IHandyTableListener tableListener, SubscriptionMode subscriptionMode = SubscriptionMode.Merge)
        {
            // Default fields to subscribe to
            var fields = new[] {
                "MID_OPEN", "HIGH", "LOW", "CHANGE", "CHANGE_PCT", "UPDATE_TIME",
                "MARKET_DELAY", "MARKET_STATE", "BID", "OFFER"
            };

            List<string> extTableInfo = new List<string>() { epic };
            return SubscribeToMarketDetails(extTableInfo, tableListener, fields, subscriptionMode); // Call the overloaded method
        }

        /// <summary>
        /// Subscribes to market details updates
        /// </summary>
        public SubscribedTableKey SubscribeToMarketDetails(IEnumerable<string> epics, IHandyTableListener tableListener, SubscriptionMode subscriptionMode = SubscriptionMode.Merge)
        {
            if (epics.Count() > 40)
            {
                throw new ArgumentOutOfRangeException("You can only subscribe to up to 40 different market streams at the same time for live data. Create a new connection to add more. Consider using the method SubscribeToMarketDetailsInBatches");
            }
            // Default fields to subscribe to
            var fields = new[] {
                "MID_OPEN", "HIGH", "LOW", "CHANGE", "CHANGE_PCT", "UPDATE_TIME",
                "MARKET_DELAY", "MARKET_STATE", "BID", "OFFER"
            };
            return SubscribeToMarketDetails(epics, tableListener, fields, subscriptionMode); // Call the overloaded method
        }
        /// <summary>
        /// Subscribes to market details updates
        /// </summary>
        public List<SubscribedTableKey> SubscribeToMarketDetailsInBatches(IEnumerable<string> epics, IHandyTableListener tableListener, SubscriptionMode subscriptionMode = SubscriptionMode.Merge)
        {
            // Batch size is 40 as per the limit in SubscribeToMarketDetails method
            const int batchSize = 40;

            // Split epics into batches of 40
            var epicBatches = epics.Select((epic, index) => new { epic, index })
                                   .GroupBy(x => x.index / batchSize)
                                   .Select(group => group.Select(x => x.epic).ToList())
                                   .ToList();

            // List to hold all subscribed keys
            var subscribedKeys = new List<SubscribedTableKey>();

            // Subscribe for each batch of epics
            foreach (var batch in epicBatches)
            {
                // Call the original SubscribeToMarketDetails method for each batch
                var subscribedKey = SubscribeToMarketDetails(batch, tableListener, subscriptionMode);
                subscribedKeys.Add(subscribedKey);
            }

            return subscribedKeys;
        }
        /// <summary>
        /// Subscribes to market details updates with specified fields
        /// </summary>
        public SubscribedTableKey SubscribeToMarketDetails(IEnumerable<string> epics, IHandyTableListener tableListener, IEnumerable<string> fields, SubscriptionMode subscriptionMode = SubscriptionMode.Merge)
        {
            string[] items = epics.Select(e => $"L1:{e}").ToArray(); // Create item names for subscription
            var extTableInfo = new ExtendedTableInfo(
                items,                       // Items to subscribe to
                subscriptionMode.GetDescription(),// Mode of subscription
                                             //     -- DISTINCT indicates that each update should yield a notification and is required for trade notifications.
                                             //     -- MERGE indicates that updates occurring very close together should only yield one update, and is used for account and price notifications to regulate the update rate) 
                fields.ToArray(),            // Fields to subscribe to
                true                         // Snapshot required
            );
            return lsClient.SubscribeTable(extTableInfo, tableListener, false); // Subscribe to the table
        }

        /// <summary>
        /// Unsubscribes from a table using the provided key
        /// </summary>
        public void UnsubscribeTableKey(SubscribedTableKey stk)
        {
            try
            {
                if (lsClient != null)
                {
                    lsClient.UnsubscribeTable(stk); // Unsubscribe from the table
                    Debug.WriteLine("Unsubscribed from table.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Exception in UnsubscribeTableKey: {ex.Message}");
            }
        }

        #endregion

        #region Implementation of IConnectionListener interface methods
        /// <summary>
        /// Called when there is an activity warning
        /// </summary>
        public void OnActivityWarning(bool warningOn)
        {
            // Log the activity warning status
            Debug.WriteLine($"Activity warning is now {(warningOn ? "on" : "off")}.");
        }

        /// <summary>
        /// Called when the connection is closed
        /// </summary>
        public void OnClose()
        {
            // Handle connection close event
            Debug.WriteLine("Connection closed by the server.");
            // Optionally, cleanup resources or attempt reconnection
            lsClient = null; // Set the client to null
        }

        /// <summary>
        /// Called when the connection is established
        /// </summary>
        public void OnConnectionEstablished()
        {
            // Handle connection established event
            Debug.WriteLine("Connection established.");
        }

        /// <summary>
        /// Called when there is a data error
        /// </summary>
        public void OnDataError(PushServerException e)
        {
            // Handle data error
            Debug.WriteLine($"Data error: {e.Message}");
            // Optionally, rethrow the exception or handle it accordingly
            throw new Exception("Data error occurred.", e);
        }

        /// <summary>
        /// Called when the connection ends
        /// </summary>
        public void OnEnd(int cause)
        {
            // Handle end of connection
            Debug.WriteLine($"Connection ended. Cause: {cause}");
            // Optionally, attempt reconnection or cleanup
            DisconnectStreaming();
        }

        /// <summary>
        /// Called when there is a connection failure
        /// </summary>
        public void OnFailure(PushConnException e)
        {
            // Handle connection failure
            Debug.WriteLine($"Connection failure: {e.Message}");
            // Optionally, attempt reconnection
            DisconnectStreaming();
            // Optionally, rethrow the exception
            throw new Exception("Connection failure occurred.", e);
        }

        /// <summary>
        /// Called when there is a server failure
        /// </summary>
        public void OnFailure(PushServerException e)
        {
            // Handle server failure
            Debug.WriteLine($"Server failure: {e.Message}");
            // Optionally, attempt reconnection
            DisconnectStreaming();
            // Optionally, rethrow the exception
            throw new Exception("Server failure occurred.", e);
        }

        /// <summary>
        /// Called when new bytes are received
        /// </summary>
        public void OnNewBytes(long bytes)
        {
            // Implement logic for handling new bytes if needed
            // For example, tracking data usage
            totalBytesReceived += bytes; // Accumulate total bytes received
            Debug.WriteLine($"New bytes received: {bytes}. Total bytes received: {totalBytesReceived}");
        }

        /// <summary>
        /// Called when the session starts
        /// </summary>
        public void OnSessionStarted(bool isPolling)
        {
            // Handle session started event
            Debug.WriteLine($"Session started. IsPolling: {isPolling}");
        }

        #endregion
    }
}
