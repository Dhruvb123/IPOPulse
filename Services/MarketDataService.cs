using IPOPulse.DBContext;
using IPOPulse.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

namespace IPOPulse.Services
{
    public class MarketDataService
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public MarketDataService(IConfiguration config, AppDBContext context)
        {
            _config = config;
            _context = context;
            _httpClient = new HttpClient();

            var apiKey = _config["MarketAPI:Key"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("API key missing.");

            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task GetMarketData()
        {
            List<IPOData> ipos = _context.Ipo.ToList<IPOData>();
            foreach (var ipo in ipos)
            {
                var curr = await _context.Market.FirstOrDefaultAsync(m => m.ID == ipo.Id);
                if (curr != null && curr.counter>2) { 
                    continue;
                }

                var baseUrl = _config["MarketAPI:BaseURL"];
                var endpoint = $"/stock?name={ipo.Symbol}";

                DateTime secondDayAfterListing = ipo.ListingDate.Date.AddDays(1);
                DateTime today = DateTime.Now.Date;

                if (today == ipo.ListingDate && DateTime.Now.Hour == 15 && DateTime.Now.Minute == 45)
                {
                    var response = await _httpClient.GetAsync(baseUrl + endpoint);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"HTTP error: {response.StatusCode}, Details: {errorContent}");
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();

                    if (jsonString.Contains("error"))
                    {
                        continue;
                    }

                    JsonNode data = JsonNode.Parse(jsonString);

                    string ISIN = data?["companyProfile"]?["isInId"]?.GetValue<string>();
                    string Name = data?["companyname"]?.GetValue<string>();
                    string currentPrice = data?["currentPrice"]?["NSE"]?.GetValue<string>();
                    string listingDayHigh = data?["yearHigh"]?.GetValue<string>();
                    string listingDayLow = data?["yearLow"]?.GetValue<string>();

                    string offeredPrice = ipo.Price;
                    int counter = 0;

                    MarketData stock = new MarketData()
                    {
                        ISIN = ISIN,
                        Name = Name,
                        offeredPrice = offeredPrice,
                        listingDayHigh = listingDayHigh,
                        listingDayLow = listingDayLow,
                        currentPrice = currentPrice,
                        counter = counter,
                        ID = ipo.Id
                    };

                    _context.Market.Add(stock);
                    await _context.SaveChangesAsync();
                }
                else if (today >= secondDayAfterListing)
                {
                    var response = await _httpClient.GetAsync(baseUrl + endpoint);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"HTTP error: {response.StatusCode}, Details: {errorContent}");
                    }

                    if (response != null)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();

                        JsonNode data = JsonNode.Parse(jsonString);

                        string price = data["currentPrice"]["NSE"].GetValue<string>();

                        var stock = await _context.Market.FirstOrDefaultAsync(m => m.ISIN == data["companyProfile"]["isInId"].GetValue<string>());

                        if (stock != null)
                        {

                            decimal priceDecimal = decimal.Parse(price, CultureInfo.InvariantCulture);
                            decimal listingDayHighDecimal = decimal.Parse(stock.listingDayHigh, CultureInfo.InvariantCulture);

                            stock.currentPrice = price;
                            if (priceDecimal > listingDayHighDecimal)
                            {
                                stock.counter = stock.counter + 1;
                            }
                            else
                            {
                                stock.counter = 0;
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
