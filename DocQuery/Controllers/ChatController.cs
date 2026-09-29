using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocQuery.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILlmService _llmService;

    public ChatController(ILlmService llmService)
    {
        _llmService = llmService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var answer = await _llmService.GenerateAsync(request.Prompt);

        return Ok(new
        {
            answer
        });
    }
}

public class ChatRequest
{
    public string Prompt { get; set; } = string.Empty;
}