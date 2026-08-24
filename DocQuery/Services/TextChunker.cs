using DocQuery.Models;

namespace DocQuery.Services;

public class TextChunker
{
    public List<DocumentChunk> ChunkPages(
        List<PdfPageContent> pages,
        int chunkSize = 1000,
        int overlap = 200)
    {
        var chunks = new List<DocumentChunk>();

        var chunkIndex = 0;

        foreach (var page in pages)
        {
            if (string.IsNullOrWhiteSpace(page.Text))
            {
                continue;
            }

            var text = page.Text;

            var start = 0;

            while (start < text.Length)
            {
                var length = Math.Min(
                    chunkSize,
                    text.Length - start);

                var chunk = text
                    .Substring(start, length)
                    .Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(new DocumentChunk
                    {
                        ChunkIndex = chunkIndex,
                        PageNumber = page.PageNumber,
                        Text = chunk
                    });

                    chunkIndex++;
                }

                start += chunkSize - overlap;
            }
        }

        return chunks;
    }
}