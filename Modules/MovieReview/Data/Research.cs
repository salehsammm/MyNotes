namespace MovieReviews.Data;

public class ResearchDoc
{
	public int Id { get; set; }
	public string Title { get; set; } = "";
	public string? Category { get; set; }
	public int SortOrder { get; set; }
	public List<ResearchSection> Sections { get; set; } = new();
}

public class ResearchSection
{
	public int Id { get; set; }
	public int ResearchDocId { get; set; }
	public ResearchDoc Doc { get; set; } = null!;

	public int? ParentId { get; set; }
	public ResearchSection? Parent { get; set; }
	public List<ResearchSection> Children { get; set; } = new();

	public string Heading { get; set; } = "";
	public string? Weight { get; set; }
	public string Body { get; set; } = "";
	public int SortOrder { get; set; }
}