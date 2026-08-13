using DocQuery.Services;

namespace DocQuery.Services;

public class RagService
{
    private readonly OllamaService _ollamaService;
    private readonly QdrantService _qdrantService;

    public RagService(
        OllamaService ollamaService,
        QdrantService qdrantService)
    {
        _ollamaService = ollamaService;
        _qdrantService = qdrantService;
    }

    public async Task<string> AskAsync(string question)
    {
        // 1. Convert question into embedding
        var queryEmbedding =
            await _ollamaService.GenerateEmbeddingAsync(question);

        // 2. Retrieve relevant chunks
        var searchResults =
            await _qdrantService.SearchAsync(
                queryEmbedding,
                3);

        // 3. Build context from retrieved chunks
        var context = string.Join(
            "\n\n",
            searchResults.Select(result =>
                $"Document: {result.Document}\n" +
                $"Content: {result.Text}"));

        // 4. Build prompt for Qwen
        var prompt = $"""
            You are a document question-answering assistant.

            Answer the user's question using only the provided context.

            If the answer cannot be found in the context,
            say that the information is not available in the documents.

            Context:
            {context}

            Question:
            {question}

            Answer:
            """;

        // 5. Send context + question to Qwen
        var answer =
            await _ollamaService.GenerateAsync(prompt);

        return answer;
    }
}