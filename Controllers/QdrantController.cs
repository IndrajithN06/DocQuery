using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocQuery.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QdrantController : ControllerBase
{
    private readonly QdrantService _qdrantService;
    private readonly OllamaService _ollamaService;

    public QdrantController(
        QdrantService qdrantService,
        OllamaService ollamaService)
    {
        _qdrantService = qdrantService;
        _ollamaService = ollamaService;
    }

    [HttpPost("create-collection")]
    public async Task<IActionResult> CreateCollection()
    {
        await _qdrantService.CreateCollectionAsync();

        return Ok(new
        {
            collection = "docquery_documents",
            message = "Collection created successfully."
        });
    }

    [HttpPost("insert")]
    public async Task<IActionResult> Insert()
    {
        const string text =
            "Employees receive 20 days of annual leave.";

        const string documentName =
            "employee-policy.txt";

        var embedding =
            await _ollamaService.GenerateEmbeddingAsync(text);

        await _qdrantService.InsertDocumentAsync(
            1,
            embedding,
            text,
            documentName);

        return Ok(new
        {
            id = 1,
            text,
            dimensions = embedding.Length,
            message = "Document embedding inserted into Qdrant."
        });
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
    [FromBody] SearchRequest request)
    {
        var queryEmbedding =
            await _ollamaService.GenerateEmbeddingAsync(request.Query);

        var results =
            await _qdrantService.SearchAsync(
                queryEmbedding,
                request.Limit);

        return Ok(results);
    }

    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;

        public ulong Limit { get; set; } = 3;
    }
}