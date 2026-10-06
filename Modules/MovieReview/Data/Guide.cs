namespace MovieReviews.Data;

public class GuideSection
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string Icon { get; set; } = "";
	public int SortOrder { get; set; }
	public List<GuideItem> Items { get; set; } = new();
}

public class GuideItem
{
	public int Id { get; set; }
	public int GuideSectionId { get; set; }
	public GuideSection Section { get; set; } = null!;
	public string Text { get; set; } = "";
	public string? Description { get; set; }
	public int SortOrder { get; set; }
}