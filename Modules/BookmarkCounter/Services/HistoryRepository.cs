using BookmarkCounter.Data;
using Microsoft.EntityFrameworkCore;

namespace BookmarkCounter.Services
{
	public class HistoryRepository
	{
		private readonly IDbContextFactory<AppDbContext> _factory;
		public HistoryRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

		public async Task InsertAsync(string folder, int count)
		{
			await using var db = await _factory.CreateDbContextAsync();
			db.BookmarkCounts.Add(new BookmarkCount
			{
				FolderName = folder,
				Count = count,
				RecordedAt = DateTime.UtcNow
			});
			await db.SaveChangesAsync();
		}

		public async Task<List<HistoryPoint>> GetHistoryAsync(string folder, int days)
		{
			await using var db = await _factory.CreateDbContextAsync();
			var since = DateTime.UtcNow.AddDays(-days);

			// Pull raw rows, then group in-memory by LOCAL day.
			var rows = await db.BookmarkCounts
				.Where(x => x.FolderName == folder && x.RecordedAt >= since)
				.OrderBy(x => x.RecordedAt)
				.Select(x => new { x.RecordedAt, x.Count })
				.ToListAsync();

			return rows
				.GroupBy(x => x.RecordedAt.ToLocalTime().Date)
				.OrderBy(g => g.Key)
				.Select(g => new HistoryPoint(
					g.Key.ToString("yyyy-MM-dd"),
					g.Last().Count))
				.ToList();
		}
	}

	public record HistoryPoint(string Day, int Count);
}
