namespace MovieReviews.Data;

public class Note
{
	public int Id { get; set; }
	public string Title { get; set; } = "";
	public string Body { get; set; } = "";
	public string? Category { get; set; }
	public int SortOrder { get; set; }
}