using IPOPulse.DBContext;
using IPOPulse.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Nodes;

namespace IPOPulse.Services
{
    public class MarketDataService
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly AlertService _alert;

        public MarketDataService(IConfiguration config, AppDBContext context, AlertService alert)
        {
            _config = config;
            _context = context;
            _httpClient = new HttpClient();

            var apiKey = _config["MarketAPI:Key"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("API key missing.");

            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _alert = alert;
        }

        public async Task GetMarketData()
        {
            try
            {
                List<MarketData> stocks = _context.Market.ToList();
                foreach (MarketData stock in stocks)
                {
                    var baseUrl = _config["MarketAPI:BaseURL"];
                    var endpoint = $"/stock?name={stock.Symbol}";

                    var response = await _httpClient.GetAsync(baseUrl + endpoint);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"HTTP error: {response.StatusCode}, Details: {errorContent}");
                    }

                    if (response != null)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        if (jsonString.Contains("error"))
                        {
                            throw new Exception($"Error response received while fetching data for {stock.Name}");
                        }

                        JsonNode data = JsonNode.Parse(jsonString);

                        string price = data["currentPrice"]["NSE"].GetValue<string>();

                        decimal priceDecimal = decimal.Parse(price, CultureInfo.InvariantCulture);
                        decimal listingDayHighDecimal = decimal.Parse(stock.listingDayHigh, CultureInfo.InvariantCulture);
                        decimal listingDayLowDecimal = decimal.Parse(stock.listingDayLow, CultureInfo.InvariantCulture);

                        stock.currentPrice = price;
                               
                        if (priceDecimal > listingDayHighDecimal)
                        {
                            // Buy Alert
                            stock.counter = stock.counter + 1;
                            await _alert.BuyAlert(stock);
                            _context.Market.Remove(stock);
                        }
                                               
                        if(priceDecimal < listingDayLowDecimal * 0.9m)
                        {
                            _context.Market.Remove(stock);
                        }                                 
                    }                                      
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) { 
                Console.WriteLine("Exception occurred while fethcin Market Data.\n" + "Error Message: " + ex.Message + "\nInner Message: " + ex.InnerException + "\n");
                return;
            }
        }
    }
}
