using DocQuery.Services;
using Microsoft.AspNetCore.Mvc;


namespace DocQuery.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly PdfService _pdfService;
    private readonly TextChunker _textChunker;
    private readonly OllamaService _ollamaService;
    private readonly QdrantService _qdrantService;

    public DocumentController(PdfService pdfService ,TextChunker textChunker,QdrantService qdrantService,OllamaService ollamaservice)
    {
        _pdfService = pdfService;
        _textChunker=textChunker;
        _qdrantService = qdrantService;
        _ollamaService = ollamaservice;

    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a PDF file.");
        }

        if (!Path.GetExtension(file.FileName)
            .Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF files are supported.");
        }

        using var stream = file.OpenReadStream();

        // 1. Extract pages
        var pages = _pdfService.ExtractPages(stream);

        // 2. Split pages into chunks
        var chunks = _textChunker.ChunkPages(pages);

        var startId = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Guid documentId = Guid.NewGuid();

        // 3. Generate embeddings and store each chunk
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];

            var embedding =
                await _ollamaService.GenerateEmbeddingAsync(chunk.Text);

            await _qdrantService.InsertDocumentAsync(
                id: startId + (ulong)i,
                embedding: embedding,
                text: chunk.Text,
                documentName: file.FileName,
                pageNumber: chunk.PageNumber,
                documentId: documentId
                );
        }

        return Ok(new
        {
            fileName = file.FileName,
            chunkCount = chunks.Count,
            message = "Document uploaded and indexed successfully.",
            documentId= documentId.ToString()
        });
    }

    [HttpGet("list-documents")]
    public async Task<IActionResult> List()
    {
        var documents = await _qdrantService.GetAllDocumentsAsync();
        return Ok(documents);
    }
}