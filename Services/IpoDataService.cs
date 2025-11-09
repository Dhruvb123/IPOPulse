
using System.Globalization;
using System.Text.Json.Nodes;
using IPOPulse.DBContext;
using IPOPulse.Models;
using Microsoft.EntityFrameworkCore;
public class IpoDataService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly AppDBContext _dbcontext;

    public IpoDataService(IConfiguration configuration, AppDBContext context)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
        _dbcontext = context;

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
                $"page=1",
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

                response = await _httpClient.GetAsync(baseURL + endpoint);

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

                            bool exists = await _dbcontext.Ipo.AnyAsync(ipo => ipo.Id == id);

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
                            _dbcontext.Ipo.Add(ipo);
                        }
                    }
                }
            }

            await _dbcontext.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            Console.WriteLine("Error fetching IPO data: " + ex.Message);
            // Exit the method without continuing further
            return;
        }
    }

}
