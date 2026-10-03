using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocQuery.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
[Route("api/[controller]")]
public class RagController : ControllerBase
{
    private readonly RagService _ragService;
    private readonly QdrantService _qdrantService;
    private readonly ICurrentUserService _currentUser;

    public RagController(
        RagService ragService,
        QdrantService qdrantService,
        ICurrentUserService currentUser)
    {
        _ragService = ragService;
        _qdrantService = qdrantService;
        _currentUser = currentUser;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        [FromBody] RagRequest request)
    {
        var userId = _currentUser.UserId;

        if (!string.IsNullOrWhiteSpace(request.DocumentId))
        {
            if (!Guid.TryParse(request.DocumentId, out var documentId) ||
                !await _qdrantService.DocumentBelongsToUserAsync(documentId, userId))
            {
                return NotFound();
            }
        }

        var answer = await _ragService.AskAsync(request.Question, request.DocumentId, userId);

        return Ok(new
        {
            question = request.Question,
            answer.Answer,
            answer.Sources
        });
    }
}

public class RagRequest
{
    public string Question { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
}
