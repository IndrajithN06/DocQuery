using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocQuery.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QdrantController : ControllerBase
{
    private readonly QdrantService _qdrantService;
    private readonly ILlmService _llmService;

    public QdrantController(
        QdrantService qdrantService,
        ILlmService llmService)
    {
        _qdrantService = qdrantService;
        _llmService = llmService;
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

    [HttpDelete("reset")]
    public async Task<IActionResult> Reset()
    {
        await _qdrantService.ResetCollectionAsync();

        return Ok(new
        {
            message = "Qdrant collection reset successfully."
        });
    }


    [HttpPost("search")]
    public async Task<IActionResult> Search(
    [FromBody] SearchRequest request)
    {
        var queryEmbedding =
            await _llmService.GenerateEmbeddingAsync(request.Query);

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