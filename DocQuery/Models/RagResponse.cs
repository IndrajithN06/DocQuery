namespace DocQuery.Models;

public class RagResponse
{
	public string Answer { get; set; } = string.Empty;

	public List<RagSource> Sources { get; set; } = new();
}

public class RagSource
{
	public string Document { get; set; } = string.Empty;

	public int PageNumber { get; set; }
}