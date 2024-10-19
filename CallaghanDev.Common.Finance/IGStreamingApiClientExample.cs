
using System;
using System.Collections.Generic;
using System.Diagnostics;
using CallaghanDev.IG.Trade.IGStreaming;
using Lightstreamer.DotNet.Client;

namespace CallaghanDev.IG.Trade
{
    /*
    class IGStreamingApiClientExample
    {
        public static void Start()
        {
            // Initialize the streaming API client
            var streamingClient = new IG.TradingAPI("","","");

            // Replace these with your actual credentials
            string username = "YOUR_USERNAME";
            string cstToken = "YOUR_CST_TOKEN";
            string xSecurityToken = "YOUR_X_SECURITY_TOKEN";
            string apiKey = "YOUR_API_KEY";
            string lsHost = "https://demo-apd.marketdatasystems.com"; // Use the correct host for your account type

            // Connect to the Lightstreamer server
            bool connected = streamingClient.Connect(username, cstToken, xSecurityToken, apiKey, lsHost);

            if (connected)
            {
                Console.WriteLine("Connected to Lightstreamer server.");

                // Subscribe to market details for a specific EPIC
                string epic = "IX.D.DAX.IFD.IP"; // Example EPIC for DAX index
                
                    Understanding EPIC Codes

                    Unique Identification: EPIC codes uniquely identify each market or instrument within the IG trading platform. This allows traders and developers to 
                    reference and interact with specific markets programmatically.

                    Format of EPIC Codes: EPIC codes are typically alphanumeric strings that may include dots or other delimiters. The format can vary depending on the 
                    type of instrument. For example:
                        Indices: "IX.D.DAX.IFD.IP" represents the Germany 30 (DAX) index.
                        Forex Pairs: "CS.D.EURUSD.MINI.IP" represents the EUR/USD currency pair.
                        Commodities: "CC.D.CL.UNC.IP" represents US Crude Oil.
                        Shares: "UA.D.AAPL.CASH.IP" represents Apple Inc. shares.

                    Usage in the API: EPIC codes are used in API calls to specify which market data or trading actions are related to which instrument.
                
                var marketListener = new MarketDetailsTableListerner();

                // Handle the Update event to process incoming market data
                marketListener.Update += MarketListener_Update;

                // Subscribe to market details using the streaming client
                var subscriptionKey = streamingClient.SubscribeToMarketDetails(
                    new List<string> { epic },
                    marketListener);

                Console.WriteLine("Subscribed to market details for EPIC: " + epic);

                // Keep the application running to receive updates
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                // Unsubscribe and disconnect when exiting
                streamingClient.UnsubscribeTableKey(subscriptionKey);
                streamingClient.Disconnect();
            }
            else
            {
                Console.WriteLine("Failed to connect to Lightstreamer server.");
            }
        }

        // Event handler for processing market data updates
        private static void MarketListener_Update(object sender, UpdateArgs<L1LsPriceData> e)
        {
            var data = e.UpdateData;
            if (data != null)
            {
                Console.WriteLine($"Update received for {e.ItemName}:");
                Console.WriteLine($"Bid: {data.Bid}, Offer: {data.Offer}, UpdateTime: {data.UpdateTime}");
                Console.WriteLine($"High: {data.High}, Low: {data.Low}, Change: {data.Change}, ChangePct: {data.ChangePct}");
                Console.WriteLine("-----------------------------------------------------");
            }
            else
            {
                Console.WriteLine("Received null update data.");
            }
        }
    }
    */
}
