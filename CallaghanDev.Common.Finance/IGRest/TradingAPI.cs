using CallaghanDev.IG.Trade.IGRest;
using Lightstreamer.DotNet.Client;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace CallaghanDev.IG
{
    public partial class TradingAPI
    {
        private readonly string apiKey;
        private readonly string identifier;
        private readonly string password;
        private readonly string lsHost;
        private readonly string ApiBase;
        private string cstToken;
        private string xSecurityToken;
        private string accountId;
        private HttpClient httpClient;

        public TradingAPI(string apiKey, string identifier, string password, string lsHost, string ApiBase)
        {
            this.apiKey = apiKey;
            this.identifier = identifier;
            this.password = password;
            this.lsHost = lsHost;
            this.ApiBase = ApiBase;
            try
            {
                lsClient = new LSClient(); // Initialize LSClient
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow it
                Debug.WriteLine($"Exception in IGStreamingApiClient constructor: {ex.Message}");
                throw;
            }
            InitializeHttpClient();
        }
        private void InitializeHttpClient()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-IG-API-KEY", apiKey);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json; charset=UTF-8");
        }

        /// <summary>
        /// Authenticates the user and retrieves session tokens using API version 2.
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            var authData = new
            {
                identifier,
                password
            };

            var content = new StringContent(JsonConvert.SerializeObject(authData), System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase}/gateway/deal/session")
            {
                Content = content
            };

            // Corrected header name to "Version"
            request.Headers.Add("Version", "2");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var authResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

                // Extract tokens from response headers
                cstToken = response.Headers.Contains("CST") ? response.Headers.GetValues("CST").FirstOrDefault() : null;
                xSecurityToken = response.Headers.Contains("X-SECURITY-TOKEN") ? response.Headers.GetValues("X-SECURITY-TOKEN").FirstOrDefault() : null;
                accountId = authResponse.currentAccountId;

                // Set tokens in default request headers for subsequent requests
                httpClient.DefaultRequestHeaders.Add("CST", cstToken);
                httpClient.DefaultRequestHeaders.Add("X-SECURITY-TOKEN", xSecurityToken);

                return true;
            }
            else
            {
                Debug.WriteLine($"Authentication failed. Status Code: {response.StatusCode}");
                Debug.WriteLine($"Error Content: {responseBody}");

                // Attempt to parse and display the error code and message
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    Debug.WriteLine($"Error Code: {errorResponse.errorCode}");
                    Debug.WriteLine($"Error Message: {errorResponse.errorMessage}");
                }
                catch (JsonException)
                {
                    Debug.WriteLine("Unable to parse error response.");
                }

                return false;
            }
        }

        /// <summary>
        /// Logs out the current session.
        /// </summary>
        public async Task LogoutAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{ApiBase}/gateway/deal/session");
            request.Headers.Add("Version", "1");
            await httpClient.SendAsync(request);
        }

        /// <summary>
        /// Retrieves account details.
        /// </summary>
        public async Task<string> GetAccountDetailsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/accounts");
            request.Headers.Add("Version", "1");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Retrieves the list of open positions.
        /// </summary>
        public async Task<string> GetOpenPositionsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/positions");
            request.Headers.Add("Version", "2");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Creates a new position (places an order).
        /// </summary>
        public async Task<string> CreatePositionAsync(string epic, string direction, decimal size, string orderType = "MARKET")
        {
            var orderData = new
            {
                epic,
                expiry = "-",
                direction,
                size,
                orderType,
                currencyCode = "USD",
                forceOpen = true,
                guaranteedStop = false
            };

            var content = new StringContent(JsonConvert.SerializeObject(orderData));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase}/gateway/deal/positions/otc")
            {
                Content = content
            };
            request.Headers.Add("Version", "2");

            // Required for dealing endpoints
            request.Headers.Add("_method", "POST");
            request.Headers.Add("Content-Type", "application/json; charset=UTF-8");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Closes an existing position.
        /// </summary>
        public async Task<string> ClosePositionAsync(string dealId, string direction, decimal size, string orderType = "MARKET")
        {
            var orderData = new
            {
                dealId,
                direction,
                size,
                orderType,
                timeInForce = "EXECUTE_AND_ELIMINATE"
            };

            var content = new StringContent(JsonConvert.SerializeObject(orderData));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase}/gateway/deal/positions/otc")
            {
                Content = content
            };
            request.Headers.Add("Version", "1");

            // Required for dealing endpoints
            request.Headers.Add("_method", "DELETE");
            request.Headers.Add("Content-Type", "application/json; charset=UTF-8");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Retrieves the list of working orders.
        /// </summary>
        public async Task<string> GetWorkingOrdersAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/workingorders");
            request.Headers.Add("Version", "2");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Creates a new working order.
        /// </summary>
        public async Task<string> CreateWorkingOrderAsync(string epic, string direction, decimal size, decimal level, string orderType = "LIMIT")
        {
            var orderData = new
            {
                epic,
                expiry = "-",
                direction,
                size,
                level,
                orderType,
                timeInForce = "GOOD_TILL_CANCELLED",
                currencyCode = "USD",
                guaranteedStop = false
            };

            var content = new StringContent(JsonConvert.SerializeObject(orderData));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase}/gateway/deal/workingorders/otc")
            {
                Content = content
            };
            request.Headers.Add("Version", "2");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Deletes an existing working order.
        /// </summary>
        public async Task<string> DeleteWorkingOrderAsync(string dealId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase}/gateway/deal/workingorders/otc");
            request.Headers.Add("Version", "1");
            request.Headers.Add("_method", "DELETE");

            var orderData = new
            {
                dealId
            };

            var content = new StringContent(JsonConvert.SerializeObject(orderData));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Retrieves market details for a specific epic.
        /// </summary>
        public async Task<string> GetMarketDetailsAsync(string epic)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/markets/{epic}");
            request.Headers.Add("Version", "3");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Searches markets by keyword.
        /// </summary>
        public async Task<List<Market>> SearchMarketsAsync(string searchTerm)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/markets?searchTerm={searchTerm}");
            request.Headers.Add("Version", "1");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

            // Deserialize the 'markets' property to List<Market>
            var marketsJson = responseObject.markets.ToString();
            return JsonConvert.DeserializeObject<List<Market>>(marketsJson);
        }
        /// <summary>
        /// Retrieves historical prices for a market.
        /// </summary>
        public async Task<string> GetHistoricalPricesAsync(string epic, string resolution = "MINUTE", int max = 10)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/prices/{epic}?resolution={resolution}&max={max}");
            request.Headers.Add("Version", "3");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetHistoricalPricesAsync(string epic, string resolution = "MINUTE", DateTime? from = null, DateTime? to = null, int max = 10)
        {
            // Construct the base URL with the epic, resolution, and max parameters
            var url = $"{ApiBase}/gateway/deal/prices/{epic}?resolution={resolution}&max={max}";

            // Add the from and to parameters if they are specified
            if (from.HasValue)
            {
                url += $"&from={from.Value.ToString("yyyy-MM-ddTHH:mm:ss")}";
            }
            if (to.HasValue)
            {
                url += $"&to={to.Value.ToString("yyyy-MM-ddTHH:mm:ss")}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Version", "3");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Confirms a trade order.
        /// </summary>
        public async Task<string> ConfirmOrderAsync(string dealReference)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/confirms/{dealReference}");
            request.Headers.Add("Version", "1");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Retrieves the transaction history.
        /// </summary>
        public async Task<string> GetTransactionHistoryAsync(string type = "ALL", int maxSpanSeconds = 86400)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/history/transactions/{type}?maxSpanSeconds={maxSpanSeconds}");
            request.Headers.Add("Version", "2");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Retrieves the activity history.
        /// </summary>
        public async Task<string> GetActivityHistoryAsync(int period = 86400)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/gateway/deal/history/activity?from={DateTime.UtcNow.AddSeconds(-period):yyyy-MM-ddTHH:mm:ss}&to={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}");
            request.Headers.Add("Version", "3");

            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
