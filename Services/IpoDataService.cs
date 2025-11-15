
using System.Globalization;
using System.Text.Json.Nodes;
using IPOPulse.DBContext;
using IPOPulse.Models;
using Microsoft.EntityFrameworkCore;
public class IpoDataService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly AppDBContext _context;

    public IpoDataService(IConfiguration configuration, AppDBContext context)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
        _context = context;

        var apiKey = _configuration["IpoAPI:Key"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("API key missing.");

        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
    }

    public async Task FetchAndSaveIpoData()
    {
        try
        {
            List<string> queryParams = new List<string>
            {
                "status=open",
                "page=5",
                "limit=1"
            };

            var baseURL = _configuration["IpoAPI:BaseURL"];
            var endpoint = $"/ipos?{string.Join("&", queryParams)}";

            var response = await _httpClient.GetAsync(baseURL + endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"HTTP error: {response.StatusCode}, Details: {errorContent}");
            }
            
            var jsonString = await response.Content.ReadAsStringAsync();
            
            JsonNode data = JsonNode.Parse(jsonString);

            int totalPages = data?["meta"]?["totalPages"]?.GetValue<int>() ?? 0;
        
            for (int page=1; page<=totalPages; page++)
            {
                queryParams = new List<string>
                {
                    "status=open",
                    $"page={page}",
                    "limit=1"
                };
                endpoint = $"/ipos?{string.Join("&", queryParams)}";

                if (page != 1)
                {
                    response = await _httpClient.GetAsync(baseURL + endpoint);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"HTTP error: {response.StatusCode}, Details: {errorContent}");
                }

                jsonString = await response.Content.ReadAsStringAsync();
                data = JsonNode.Parse(jsonString);

                var iposArray = data?["ipos"]?.AsArray();

                if (iposArray != null)
                {
                    foreach (var ipoNode in iposArray)
                    {
                        var ipoObject = ipoNode?.AsObject();
                        if (ipoObject != null)
                        {
                            string type = ipoObject["type"]?.GetValue<string>();
                            string id = ipoObject["id"]?.GetValue<string>();

                            bool exists = await _context.Ipo.AnyAsync(ipo => ipo.Id == id);

                            if (type == "SME" || exists) {
                                continue;
                            }
                            
                            string name = ipoObject["name"]?.GetValue<string>();
                            string symbol = ipoObject["symbol"]?.GetValue<string>();                           
                            string listingDate = ipoObject["listingDate"]?.GetValue<string>();
                            string priceRange = ipoObject["priceRange"]?.GetValue<string>();

                            IPOData ipo = new IPOData()
                            {
                                Id = id,
                                Name = name,
                                Symbol = symbol,
                                ListingDate = DateTime.ParseExact(listingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                Price = priceRange.Split("-")[1]
                            };
                            _context.Ipo.Add(ipo);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            await CheckListing();

        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            Console.WriteLine("Error fetching IPO data: " + ex.Message);
            // Exit the method without continuing further
            return;
        }
    }

    public async Task CheckListing()
    {
        try
        {
            List<IPOData> ipos = _context.Ipo.ToList();
            foreach (IPOData ipo in ipos)
            {
                DateTime today = DateTime.Now.Date;

                if (today == ipo.ListingDate)
                {
                    var baseUrl = _configuration["MarketAPI:BaseURL"];
                    var endpoint = $"/stock?name={ipo.Symbol}";
                    var response = await _httpClient.GetAsync(baseUrl + endpoint);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"HTTP error: {response.StatusCode}, Details: {errorContent}");
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();

                    if (jsonString.Contains("error"))
                    {
                        throw new Exception($"Error response received while fetching data for {ipo.Name}");
                    }

                    JsonNode data = JsonNode.Parse(jsonString);

                    string ISIN = data["companyProfile"]["isInId"].GetValue<string>();
                    string Name = data?["companyName"]?.GetValue<string>();
                    string currentPrice = data["currentPrice"]["NSE"].GetValue<string>();
                    string listingDayHigh = data["stockDetailsReusableData"]["high"].GetValue<string>();
                    string listingDayLow = data["stockDetailsReusableData"]["low"].GetValue<string>();

                    string offeredPrice = ipo.Price;
                    int counter = 0;

                    string Symbol = ipo.Symbol;

                    MarketData stock = new MarketData()
                    {
                        ISIN = ISIN,
                        Name = Name,
                        Symbol = Symbol,
                        offeredPrice = offeredPrice,
                        listingDayHigh = listingDayHigh,
                        listingDayLow = listingDayLow,
                        currentPrice = currentPrice,
                        counter = counter,
                        ID = ipo.Id,
                        ListingDate = ipo.ListingDate,
                    };

                    _context.Market.Add(stock);
                    await _context.SaveChangesAsync();
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("Error fetching IPO data: " + ex.Message);
            return;
        }
    }

}
