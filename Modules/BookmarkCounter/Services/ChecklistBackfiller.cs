namespace BookmarkCounter.Services;

public class ChecklistBackfiller : BackgroundService
{
	private readonly ChecklistRepository _repo;
	private readonly ILogger<ChecklistBackfiller> _log;

	public ChecklistBackfiller(ChecklistRepository repo, ILogger<ChecklistBackfiller> log)
	{
		_repo = repo; _log = log;
	}

	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		await SafeRun();
		using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
		while (await timer.WaitForNextTickAsync(ct))
			await SafeRun();
	}

	private async Task SafeRun()
	{
		try
		{
			var added = await _repo.EnsureRowsAsync();
			if (added > 0) _log.LogInformation("Backfilled {Count} checklist rows", added);
		}
		catch (Exception ex) { _log.LogError(ex, "Checklist backfill failed"); }
	}
}