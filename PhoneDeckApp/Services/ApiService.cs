using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PhoneDeckApp.Services;

public class ApiService
{
    private readonly HttpClient _client = new();
    private const string BaseUrl = "http://109.73.206.169";

    public void SetToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/api/login",
            new { username, password });

        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return result?["token"];
    }

    public async Task<List<Dictionary<string, object>>?> GetSessionsAsync()
    {
        var response = await _client.GetAsync($"{BaseUrl}/get_data");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content
            .ReadFromJsonAsync<List<Dictionary<string, object>>>();
    }
}