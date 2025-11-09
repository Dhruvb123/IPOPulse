using IPOPulse.DBContext;
using IPOPulse.Models;
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
            foreach(var ipo in ipos)
            {
                var baseUrl = _config["MarketAPI:BaseURL"];
                var endpoint = $"/stock?name={ipo.Symbol}";

                var response = await _httpClient.GetAsync(baseUrl+endpoint);
                if (response != null) {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(response);

                    JsonNode data = JsonNode.Parse(jsonString);

                    
                    Console.WriteLine(data);
                }
            }
        }
    }
}
