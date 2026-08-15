using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocQuery.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly OllamaService _ollamaService;

    public ChatController(OllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var answer = await _ollamaService.GenerateAsync(request.Prompt);

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