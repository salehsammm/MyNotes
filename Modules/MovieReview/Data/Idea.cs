namespace MovieReviews.Data;

public class Idea
{
	public int Id { get; set; }
	public string Text { get; set; } = "";
	public bool IsDone { get; set; }
	public int SortOrder { get; set; }
}