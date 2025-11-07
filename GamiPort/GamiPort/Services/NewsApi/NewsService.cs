using GamiPort.Models.NewsApi;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GamiPort.Services.NewsApi
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _newsApiKey;

        public NewsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _newsApiKey = _configuration["NewsApi:ApiKey"]; // Retrieve API key from appsettings.json
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GamiPort-News-Integration/1.0"); // Add User-Agent header
        }

        public async Task<List<Article>> GetTopHeadlinesAsync(string country = null, string category = null, string q = null, string sources = null, int pageSize = 5)
        {
            if (string.IsNullOrEmpty(_newsApiKey))
            {
                throw new InvalidOperationException("NewsAPI Key is not configured.");
            }

            var url = $"https://newsapi.org/v2/top-headlines?pageSize={pageSize}&apiKey={_newsApiKey}";
            if (!string.IsNullOrEmpty(country)) url += $"&country={country}";
            if (!string.IsNullOrEmpty(category)) url += $"&category={category}";
            if (!string.IsNullOrEmpty(q)) url += $"&q={System.Net.WebUtility.UrlEncode(q)}"; // URL encode the query
            if (!string.IsNullOrEmpty(sources)) url += $"&sources={sources}"; // ADD THIS LINE

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code

            var jsonString = await response.Content.ReadAsStringAsync();
            var newsApiResponse = JsonSerializer.Deserialize<NewsApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return newsApiResponse?.Articles ?? new List<Article>();
        }
    }
}
