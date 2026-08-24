using System.Net.Http.Json;

namespace DocQuery.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var request = new
        {
            model = "qwen2.5:3b",
            prompt = prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "api/generate",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<OllamaResponse>();

        return result?.Response ?? string.Empty;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text) 
    {
        var request = new
        {
            model = "nomic-embed-text",
            prompt = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            "api/embeddings",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<OllamaEmbeddingResponse>();

        return result?.Embedding ?? [];
    }

    private class OllamaEmbeddingResponse
    {
        public float[] Embedding { get; set; } = [];
    }
    private class OllamaResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}