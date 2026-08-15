namespace DocQuery.Services;

public class TextChunker
{
    public List<string> ChunkText(
        string text,
        int chunkSize = 1000,
        int overlap = 200)
    {
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
        {
            return chunks;
        }

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
                chunks.Add(chunk);
            }

            start += chunkSize - overlap;
        }

        return chunks;
    }
}