namespace BookmarkCounter.Data;

public enum RecurrenceType { Daily = 0, Weekly = 1 }

public class ChecklistItem
{
	public int Id { get; set; }
	public string Title { get; set; } = "";
	public RecurrenceType Recurrence { get; set; }
	public bool IsOneTime { get; set; }
	public bool IsOptional { get; set; }
	public DateOnly? ScheduledPeriodStart { get; set; }
	public int RepeatEveryDays { get; set; } = 1;
	public DateOnly? FirstDueDate { get; set; }
	public int TargetCount { get; set; } = 1;
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public List<ChecklistProgress> Progress { get; set; } = new();
}
