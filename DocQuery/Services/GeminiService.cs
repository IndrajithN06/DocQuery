using System.Net.Http.Json;
using System.Text.Json;

namespace DocQuery.Services;

public class GeminiService : ILlmService
{
    private readonly HttpClient _http;
    private readonly string _chatModel;
    private readonly string _embeddingModel;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        var apiKey = config["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini:ApiKey missing");
        _http = http;
        _http.DefaultRequestHeaders.TryAddWithoutValidation("x-goog-api-key", apiKey);
        _chatModel = config["Gemini:ChatModel"] ?? "gemini-3.5-flash-lite";
        _embeddingModel = config["Gemini:EmbeddingModel"] ?? "gemini-embedding-2";
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var body = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
        var response = await _http.PostAsJsonAsync($"v1beta/models/{_chatModel}:generateContent", body);
        await EnsureOk(response);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("candidates")[0]
            .GetProperty("content").GetProperty("parts")[0]
            .GetProperty("text").GetString() ?? string.Empty;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var body = new
        {
            model = $"models/{_embeddingModel}",
            content = new { parts = new[] { new { text } } },
            outputDimensionality = 768
        };
        var response = await _http.PostAsJsonAsync($"v1beta/models/{_embeddingModel}:embedContent", body);
        await EnsureOk(response);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        var values = root.TryGetProperty("embedding", out var single)
            ? single.GetProperty("values")
            : root.GetProperty("embeddings")[0].GetProperty("values");

        return values.EnumerateArray().Select(v => v.GetSingle()).ToArray();
    }

    private static async Task EnsureOk(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Gemini {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }
}