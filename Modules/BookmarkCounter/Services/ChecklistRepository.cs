using BookmarkCounter.Data;
using Microsoft.EntityFrameworkCore;

namespace BookmarkCounter.Services;

public record ChecklistItemDto(
	int Id, int ProgressId, string Title, string Recurrence,
	int TargetCount, int CurrentCount, bool Completed, string PeriodStart,
	bool IsOneTime, int RepeatEveryDays, string? FirstDueDate, bool IsOptional);

public record ChecklistRequest(string Title, RecurrenceType Recurrence, int TargetCount,
	bool IsOneTime = false, DateOnly? ScheduledDate = null,
	int RepeatEveryDays = 1, DateOnly? FirstDueDate = null, bool IsOptional = false);

public class ChecklistRepository
{
	private readonly IDbContextFactory<AppDbContext> _factory;
	public ChecklistRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

	// Saturday = start of week. Change to DayOfWeek.Monday here if you want Monday.
	public static DateOnly WeekStart(DateOnly date)
	{
		int daysSinceSaturday = ((int)date.DayOfWeek + 1) % 7;
		return date.AddDays(-daysSinceSaturday);
	}

	private static DateOnly FirstDue(ChecklistItem item) => item.FirstDueDate
		?? DateOnly.FromDateTime(item.CreatedAt.ToLocalTime());

	private static bool IsDue(ChecklistItem item, DateOnly date)
	{
		var first = FirstDue(item);
		return date >= first && (date.DayNumber - first.DayNumber) % Math.Max(1, item.RepeatEveryDays) == 0;
	}

	// Backfill missing progress rows for all active items. Returns rows added.
	public async Task<int> EnsureRowsAsync()
	{
		await using var db = await _factory.CreateDbContextAsync();
		var items = await db.ChecklistItems.Where(i => i.IsActive).ToListAsync();
		return await EnsureRowsInternal(db, items, DateOnly.FromDateTime(DateTime.Now));
	}

	private static async Task<int> EnsureRowsInternal(
		AppDbContext db, List<ChecklistItem> items, DateOnly today)
	{
		var horizon = today.AddDays(-60);
		int added = 0;

		var existing = await db.ChecklistProgress
			.Select(p => new { p.ItemId, p.PeriodStart })
			.ToListAsync();
		var existingSet = existing.Select(x => (x.ItemId, x.PeriodStart)).ToHashSet();

		foreach (var item in items)
		{
			if (item.IsOneTime)
			{
				if (item.ScheduledPeriodStart is DateOnly scheduled && existingSet.Add((item.Id, scheduled)))
				{
					db.ChecklistProgress.Add(new ChecklistProgress
					{ ItemId = item.Id, PeriodStart = scheduled });
					added++;
				}
				continue;
			}
			var created = DateOnly.FromDateTime(item.CreatedAt.ToLocalTime());

			if (item.Recurrence == RecurrenceType.Daily)
			{
				var firstDue = FirstDue(item);
				var from = firstDue > horizon ? firstDue : horizon;
				for (var d = from; d <= today; d = d.AddDays(1))
				{
					if (!IsDue(item, d)) continue;
					if (existingSet.Add((item.Id, d)))
					{
						db.ChecklistProgress.Add(new ChecklistProgress
						{ ItemId = item.Id, PeriodStart = d, CurrentCount = 0 });
						added++;
					}
				}
			}
			else
			{
				var startWeek = WeekStart(created > horizon ? created : horizon);
				var thisWeek = WeekStart(today);
				for (var w = startWeek; w <= thisWeek; w = w.AddDays(7))
				{
					if (existingSet.Add((item.Id, w)))
					{
						db.ChecklistProgress.Add(new ChecklistProgress
						{ ItemId = item.Id, PeriodStart = w, CurrentCount = 0 });
						added++;
					}
				}
			}
		}

		if (added > 0) await db.SaveChangesAsync();
		return added;
	}

	// Get items for a specific date:
	//   daily items  -> progress for that exact date
	//   weekly items -> progress for the week containing that date
	public async Task<List<ChecklistItemDto>> GetForDateAsync(DateOnly date)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var items = await db.ChecklistItems
			.Where(i => i.IsActive)
			.OrderBy(i => i.Id)
			.ToListAsync();

		var today = DateOnly.FromDateTime(DateTime.Now);
		await EnsureRowsInternal(db, items, today);

		var weekStart = WeekStart(date);
		if (date > today)
		{
			var futureItems = items.Where(i => !i.IsOneTime).ToList();
			var futureIds = futureItems.Select(i => i.Id).ToList();
			var existingFuture = await db.ChecklistProgress
				.Where(p => futureIds.Contains(p.ItemId) && (p.PeriodStart == date || p.PeriodStart == weekStart))
				.Select(p => new { p.ItemId, p.PeriodStart }).ToListAsync();
			var existingSet = existingFuture.Select(p => (p.ItemId, p.PeriodStart)).ToHashSet();
			foreach (var item in futureItems)
			{
				var period = item.Recurrence == RecurrenceType.Daily ? date : weekStart;
				if (item.Recurrence == RecurrenceType.Daily && !IsDue(item, date)) continue;
				if (existingSet.Add((item.Id, period)))
					db.ChecklistProgress.Add(new ChecklistProgress { ItemId = item.Id, PeriodStart = period });
			}
			await db.SaveChangesAsync();
		}
		var dailyIds = items.Where(i => i.Recurrence == RecurrenceType.Daily).Select(i => i.Id).ToList();
		var weeklyIds = items.Where(i => i.Recurrence == RecurrenceType.Weekly).Select(i => i.Id).ToList();

		var rows = new List<ChecklistProgress>();
		if (dailyIds.Count > 0)
			rows.AddRange(await db.ChecklistProgress
				.Where(p => dailyIds.Contains(p.ItemId) && p.PeriodStart == date)
				.ToListAsync());
		if (weeklyIds.Count > 0)
			rows.AddRange(await db.ChecklistProgress
				.Where(p => weeklyIds.Contains(p.ItemId) && p.PeriodStart == weekStart)
				.ToListAsync());

		var byItem = rows.ToDictionary(r => r.ItemId);

		var result = new List<ChecklistItemDto>();
		foreach (var item in items)
		{
			var created = DateOnly.FromDateTime(item.CreatedAt.ToLocalTime());
			var periodStart = item.Recurrence == RecurrenceType.Daily ? date : weekStart;
			if (item.IsOneTime && item.ScheduledPeriodStart != periodStart) continue;
			if (!item.IsOneTime && item.Recurrence == RecurrenceType.Daily && !IsDue(item, date)) continue;

			// Skip if the item didn't exist yet at this period
			if (!item.IsOneTime && item.Recurrence == RecurrenceType.Daily && created > date) continue;
			if (!item.IsOneTime && item.Recurrence == RecurrenceType.Weekly && WeekStart(created) > weekStart) continue;

			byItem.TryGetValue(item.Id, out var row);
			var cur = row?.CurrentCount ?? 0;
			result.Add(new ChecklistItemDto(
				item.Id, row?.Id ?? 0, item.Title, item.Recurrence.ToString(),
				item.TargetCount, cur, cur >= item.TargetCount,
				periodStart.ToString("yyyy-MM-dd"), item.IsOneTime,
				item.RepeatEveryDays, item.Recurrence == RecurrenceType.Daily && !item.IsOneTime
					? FirstDue(item).ToString("yyyy-MM-dd") : null, item.IsOptional));
		}
		return result;
	}

	public async Task<ChecklistItemDto> CreateAsync(ChecklistRequest req)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var item = new ChecklistItem
		{
			Title = req.Title.Trim(),
			Recurrence = req.Recurrence,
			IsOneTime = req.IsOneTime,
			IsOptional = req.IsOptional,
			ScheduledPeriodStart = req.IsOneTime ? SchedulePeriod(req) : null,
			RepeatEveryDays = !req.IsOneTime && req.Recurrence == RecurrenceType.Daily
				? req.RepeatEveryDays : 1,
			FirstDueDate = !req.IsOneTime && req.Recurrence == RecurrenceType.Daily
				? req.FirstDueDate ?? DateOnly.FromDateTime(DateTime.Now) : null,
			TargetCount = Math.Max(1, req.TargetCount)
		};
		db.ChecklistItems.Add(item);
		await db.SaveChangesAsync();

		await EnsureRowsInternal(db, new List<ChecklistItem> { item },
			DateOnly.FromDateTime(DateTime.Now));

		return new ChecklistItemDto(
			item.Id, 0, item.Title, item.Recurrence.ToString(),
			item.TargetCount, 0, false,
			item.ScheduledPeriodStart?.ToString("yyyy-MM-dd") ?? "", item.IsOneTime,
			item.RepeatEveryDays, item.FirstDueDate?.ToString("yyyy-MM-dd"), item.IsOptional);
	}

	public async Task<bool> UpdateAsync(int id, ChecklistRequest req)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var item = await db.ChecklistItems.FirstOrDefaultAsync(i => i.Id == id);
		if (item is null) return false;
		item.Title = req.Title.Trim();
		item.TargetCount = Math.Max(1, req.TargetCount);
		item.IsOptional = req.IsOptional;
		var wasOneTime = item.IsOneTime;
		item.IsOneTime = req.IsOneTime;
		var newScheduledPeriod = req.IsOneTime ? SchedulePeriod(req) : (DateOnly?)null;
		if (wasOneTime != req.IsOneTime)
		{
			// Keep progress at the chosen date and completed history; discard other open occurrences.
			var oldOpen = await db.ChecklistProgress
				.Where(p => p.ItemId == id && p.CurrentCount < item.TargetCount
					&& (newScheduledPeriod == null || p.PeriodStart != newScheduledPeriod))
				.ToListAsync();
			db.ChecklistProgress.RemoveRange(oldOpen);
		}
		item.ScheduledPeriodStart = newScheduledPeriod;
		item.RepeatEveryDays = !req.IsOneTime && item.Recurrence == RecurrenceType.Daily ? req.RepeatEveryDays : 1;
		item.FirstDueDate = !req.IsOneTime && item.Recurrence == RecurrenceType.Daily
			? req.FirstDueDate ?? FirstDue(item) : null;
		if (!item.IsOneTime && item.Recurrence == RecurrenceType.Daily && wasOneTime == item.IsOneTime)
		{
			var openRows = await db.ChecklistProgress
				.Where(p => p.ItemId == id && p.CurrentCount < item.TargetCount)
				.ToListAsync();
			db.ChecklistProgress.RemoveRange(openRows.Where(p => !IsDue(item, p.PeriodStart)));
		}
		if (item.IsOneTime && wasOneTime && req.ScheduledDate is not null)
		{
			var scheduled = item.Recurrence == RecurrenceType.Weekly
				? WeekStart(req.ScheduledDate.Value) : req.ScheduledDate.Value;
			item.ScheduledPeriodStart = scheduled;
			var row = await db.ChecklistProgress.FirstOrDefaultAsync(p => p.ItemId == id && p.PeriodStart == scheduled);
			if (row is null)
			{
				var oldRow = await db.ChecklistProgress.FirstOrDefaultAsync(p => p.ItemId == id && p.PeriodStart != scheduled);
				if (oldRow is not null) oldRow.PeriodStart = scheduled;
			}
		}
		await db.SaveChangesAsync();
		await EnsureRowsInternal(db, new List<ChecklistItem> { item }, DateOnly.FromDateTime(DateTime.Now));
		return true;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var item = await db.ChecklistItems.FirstOrDefaultAsync(i => i.Id == id);
		if (item is null) return false;
		db.ChecklistItems.Remove(item);
		await db.SaveChangesAsync();
		return true;
	}

	public Task<ChecklistItemDto?> IncrementAsync(int progressId) => ChangeAsync(progressId, +1);
	public Task<ChecklistItemDto?> DecrementAsync(int progressId) => ChangeAsync(progressId, -1);

	public async Task<List<ChecklistItemDto>> GetMissedAsync()
	{
		await EnsureRowsAsync();
		await using var db = await _factory.CreateDbContextAsync();
		var today = DateOnly.FromDateTime(DateTime.Now);
		var thisWeek = WeekStart(today);
		var rows = await db.ChecklistProgress.Include(p => p.Item)
			.Where(p => p.Item.IsActive && !p.Item.IsOptional && p.CurrentCount < p.Item.TargetCount
				&& (!p.Item.IsOneTime || p.Item.ScheduledPeriodStart == p.PeriodStart)
				&& ((p.Item.Recurrence == RecurrenceType.Daily && p.PeriodStart < today)
					|| (p.Item.Recurrence == RecurrenceType.Weekly && p.PeriodStart < thisWeek)))
			.OrderByDescending(p => p.PeriodStart).ThenBy(p => p.ItemId)
			.ToListAsync();
		return rows.Select(p => new ChecklistItemDto(p.ItemId, p.Id, p.Item.Title,
			p.Item.Recurrence.ToString(), p.Item.TargetCount, p.CurrentCount, false,
			p.PeriodStart.ToString("yyyy-MM-dd"), p.Item.IsOneTime,
			p.Item.RepeatEveryDays, p.Item.Recurrence == RecurrenceType.Daily && !p.Item.IsOneTime
				? FirstDue(p.Item).ToString("yyyy-MM-dd") : null, p.Item.IsOptional)).ToList();
	}

	private static DateOnly SchedulePeriod(ChecklistRequest req)
	{
		var date = req.ScheduledDate ?? DateOnly.FromDateTime(DateTime.Now);
		return req.Recurrence == RecurrenceType.Weekly ? WeekStart(date) : date;
	}

	private async Task<ChecklistItemDto?> ChangeAsync(int progressId, int delta)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var row = await db.ChecklistProgress.Include(p => p.Item)
			.FirstOrDefaultAsync(p => p.Id == progressId);
		if (row is null) return null;

		row.CurrentCount = Math.Clamp(row.CurrentCount + delta, 0, row.Item.TargetCount);
		row.CompletedAt = row.CurrentCount >= row.Item.TargetCount ? DateTime.UtcNow : null;
		await db.SaveChangesAsync();

		return new ChecklistItemDto(
			row.Item.Id, row.Id, row.Item.Title, row.Item.Recurrence.ToString(),
			row.Item.TargetCount, row.CurrentCount,
			row.CurrentCount >= row.Item.TargetCount,
			row.PeriodStart.ToString("yyyy-MM-dd"), row.Item.IsOneTime,
			row.Item.RepeatEveryDays, row.Item.Recurrence == RecurrenceType.Daily && !row.Item.IsOneTime
				? FirstDue(row.Item).ToString("yyyy-MM-dd") : null, row.Item.IsOptional);
	}
}
