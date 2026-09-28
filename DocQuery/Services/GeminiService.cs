using System.Net.Http.Json;
using System.Text.Json;

namespace DocQuery.Services;

public class GeminiService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini:ApiKey missing");
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var url = $"v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } }
        };

        var response = await _httpClient.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var url = $"v1beta/models/text-embedding-004:embedContent?key={_apiKey}";
        var body = new
        {
            model = "models/text-embedding-004",
            content = new { parts = new[] { new { text } } },
            outputDimensionality = 768   // keeps parity with your existing Qdrant collection
        };

        var response = await _httpClient.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(v => v.GetSingle())
            .ToArray();
    }
}