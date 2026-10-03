using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocQuery.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
[Route("api/[controller]")]
public class QdrantController : ControllerBase
{
    private readonly QdrantService _qdrantService;
    private readonly ILlmService _llmService;
    private readonly ICurrentUserService _currentUser;

    public QdrantController(
        QdrantService qdrantService,
        ILlmService llmService,
        ICurrentUserService currentUser)
    {
        _qdrantService = qdrantService;
        _llmService = llmService;
        _currentUser = currentUser;
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

    [HttpPost("search")]
    public async Task<IActionResult> Search(
    [FromBody] SearchRequest request)
    {
        var queryEmbedding =
            await _llmService.GenerateEmbeddingAsync(request.Query);

        var results =
            await _qdrantService.SearchAsync(
                queryEmbedding,
                request.Limit,
                userId: _currentUser.UserId);

        return Ok(results);
    }

    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;

        public ulong Limit { get; set; } = 3;
    }
}
