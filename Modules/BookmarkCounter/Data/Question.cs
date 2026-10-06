namespace BookmarkCounter.Data;

public class Question
{
	public int Id { get; set; }
	public string Text { get; set; } = "";
	public string? Answer { get; set; }
	public bool IsAnswered { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? AnsweredAt { get; set; }
}
