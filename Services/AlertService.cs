using IPOPulse.DBContext;
using IPOPulse.Models;
using System.Text.Json.Nodes;

namespace IPOPulse.Services
{
    public class AlertService
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly IMessageService _messageService;

        public AlertService(AppDBContext context, IConfiguration config, IMessageService messageService)
        {
            _context = context;
            _config = config;
            _httpClient = new HttpClient();
            _messageService = messageService;
            var apiKey = _config["MarketAPI:Key"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("API key missing.");

            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task UpdateCurrentPrice()
        {
            try
            {
                List<BStockData> stocks = _context.BStocks
                                                  .Where(stock => stock.ExitPrice == null)
                                                  .ToList(); ;
                foreach (var bstock in stocks)
                {
                    var baseUrl = _config["MarketAPI:BaseURL"];
                    var endpoint = $"/stock?name={bstock.Symbol}";

                    var response = await _httpClient.GetAsync(baseUrl + endpoint);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"HTTP error: {response.StatusCode}, Details: {errorContent}");
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();

                    if (jsonString.Contains("error"))
                    {
                        throw new Exception($"Error response received while fetching data for {bstock.Name}");
                    }

                    JsonNode data = JsonNode.Parse(jsonString);

                    bstock.CurrentPrice = data["currentPrice"]["NSE"].GetValue<string>();
                    var buy = decimal.Parse(bstock.BuyingPrice);
                    var cur = decimal.Parse(bstock.CurrentPrice);
                    var change = (cur - buy) / buy;
                    change *= 100;
                    change = Math.Round(change, 2);

                    bstock.Returns = change + "%";
                                       
                }

                await _context.SaveChangesAsync();
                await CheckSLAndTarget();
            }
            catch (Exception ex) {
                Console.WriteLine("Exception occurred while fethcin Market Data.\n" + "Error Message: " + ex.Message + "\nInner Message: " + ex.InnerException + "\n");
            }
        }

        public async Task CheckSLAndTarget()
        {
            List<BStockData> list = _context.BStocks
                                            .Where(stock => stock.ExitPrice == null)
                                            .ToList();

            foreach (var item in list)
            {
                var curr = decimal.Parse(item.CurrentPrice);
                if ( curr < decimal.Parse(item.SL))
                {
                    await SellAlert(item, 0);
                }
                var bPrice = decimal.Parse(item.BuyingPrice);
                if (curr > bPrice + (0.25m * bPrice)) { 
                    await SellAlert(item, 1);
                }

            }
        }

        public async Task BuyAlert(MarketData stock)
        {
            try
            {
                BStockData bstock = new BStockData()
                {
                    Id = stock.ID,
                    Name = stock.Name,
                    Symbol = stock.Symbol,
                    BuyingPrice = stock.currentPrice,
                    Date = DateTime.Now,
                    CurrentPrice = stock.currentPrice,
                    Returns = "0%",
                    SL = stock.listingDayLow
                };

                _context.BStocks.Add(bstock);
                await _context.SaveChangesAsync();

                string subject = "Opportunity: Stock on a Strong Bull Run – Act Now!";
                
                await _messageService.SendMailAsync(subject, stock.Name, stock.Symbol, stock.currentPrice);
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task SellAlert(BStockData stock, int indicator)
        {
            // indicator 0 for hitting SL, 1 for profit booking
            try
            {
                if (indicator == 0)
                {
                    // Fn to sell alert msg done to SL hit
                    string subject = "Alert: Time to Sell, Support Level Broken";
                    
                    await _messageService.SendMailAsync(subject, stock.Name, stock.Symbol, stock.CurrentPrice);
                }
                else
                {
                    // Sell Recommendation Msg for Profit Booking
                    string subject = "Time To Book Profits ;)";
                
                    await _messageService.SendMailAsync(subject, stock.Name, stock.Symbol, stock.CurrentPrice);
                }
                stock.ExitPrice = stock.CurrentPrice;
                await _context.SaveChangesAsync();
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"Error: {ex.ToString()}");
                throw;
            }
        }
    }
}
