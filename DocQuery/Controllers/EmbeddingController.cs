using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocQuery.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmbeddingController : ControllerBase
{
    private readonly ILlmService _llmService;

    public EmbeddingController(ILlmService llmService)
    {
        _llmService = llmService;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateEmbedding(
        [FromBody] EmbeddingRequest request)
    {
        var embedding =
            await _llmService.GenerateEmbeddingAsync(request.Text);

        return Ok(new
        {
            dimensions = embedding.Length,
            embedding
        });
    }
}

public class EmbeddingRequest
{
    public string Text { get; set; } = string.Empty;
}