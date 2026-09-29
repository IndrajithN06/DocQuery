namespace DocQuery.Services
{
    public interface ILlmService
    {
        Task<string> GenerateAsync(string prompt);
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}
