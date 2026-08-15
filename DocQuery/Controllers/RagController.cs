using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocQuery.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RagController : ControllerBase
{
    private readonly RagService _ragService;

    public RagController(RagService ragService)
    {
        _ragService = ragService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        [FromBody] RagRequest request)
    {
        var answer =
            await _ragService.AskAsync(request.Question);

        return Ok(new
        {
            question = request.Question,
            answer
        });
    }
}

public class RagRequest
{
    public string Question { get; set; } = string.Empty;
}