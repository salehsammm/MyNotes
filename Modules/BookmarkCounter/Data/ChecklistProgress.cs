namespace BookmarkCounter.Data;

public class ChecklistProgress
{
	public int Id { get; set; }
	public int ItemId { get; set; }
	public ChecklistItem Item { get; set; } = null!;
	public DateOnly PeriodStart { get; set; }   // day for Daily, Saturday for Weekly
	public int CurrentCount { get; set; }
	public DateTime? CompletedAt { get; set; }
}