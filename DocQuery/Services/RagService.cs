using DocQuery.Services;
using DocQuery.Models;
namespace DocQuery.Services;

public class RagService
{
    private readonly ILlmService _llmService;
    private readonly QdrantService _qdrantService;

    public RagService(
        ILlmService llmService,
        QdrantService qdrantService)
    {
        _llmService = llmService;
        _qdrantService = qdrantService;
    }

    public async Task<RagResponse> AskAsync(string question, string documentId, string userId)
    {
        // 1. Convert question into embedding
        var queryEmbedding =
            await _llmService.GenerateEmbeddingAsync(question);

        // 2. Retrieve relevant chunks
        var searchResults =
            await _qdrantService.SearchAsync(
                queryEmbedding,
                3,
                documentId,
                userId);

        // 3. Build context from retrieved chunks
        var context = string.Join(
            "\n\n",
            searchResults.Select(result =>
                $"Document: {result.Document}\n" +
                $"Page: {result.PageNumber}\n" +
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
            await _llmService.GenerateAsync(prompt);
        var sources=searchResults.Select(result => new RagSource
        {
            Document = result.Document,
            PageNumber = result.PageNumber
        }).DistinctBy(source => new { source.Document, source.PageNumber })
         .ToList();

        return new RagResponse { Answer = answer, Sources = sources };
    }
}
