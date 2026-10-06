namespace BookmarkCounter.Data
{
	public class BookmarkCount
	{
		public int Id { get; set; }
		public string FolderName { get; set; } = "";
		public int Count { get; set; }
		public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
	}
}
