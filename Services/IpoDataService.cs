using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class IpoDataService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public IpoDataService(IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();

        var apiKey = _configuration["API:Key"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("API key missing.");

        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
    }

    public async Task FetchAllOpenIposAndSaveToFileAsync()
    {
        var baseUrl = _configuration["API:BaseURL"];
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string outputFile = Path.Combine(desktopPath, "IPOData.txt");

        int page = 1;
        bool moreData = true;

        using (StreamWriter writer = new StreamWriter(outputFile, append: false))
        {
            while (moreData)
            {
                var endpoint = $"/ipos?status=open&limit=1&page={page}";
                var response = await _httpClient.GetAsync(baseUrl + endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API Error: {response.StatusCode}, Details: {error}");
                }

                var content = await response.Content.ReadAsStringAsync();

                // Check if content has actual IPO data or empty result
                if (IsEmptyResult(content))
                {
                    moreData = false;
                    break;
                }

                // Write the raw JSON or parse and format as needed
                await writer.WriteLineAsync($"Page {page} IPO Data:");
                await writer.WriteLineAsync(content);
                await writer.WriteLineAsync();

                page++;
            }
        }
    }

    private bool IsEmptyResult(string jsonContent)
    {
        // Simple heuristic: the API may return empty list or some indication of no data
        // Adjust depending on actual response format, e.g. check if 'data' array is empty
        return jsonContent.Contains("\"data\":[]") || jsonContent.Contains("\"status\":\"404\"");
    }
}
