namespace DocQuery.Models
{
    public class DocumentChunk
    {
        public int ChunkIndex { get; set; }
        public int PageNumber { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
