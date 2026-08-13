namespace DocQuery.Models;

public class SearchResult
{
    public string Text { get; set; } = string.Empty;

    public string Document { get; set; } = string.Empty;

    public float Score { get; set; }
}