namespace BookmarkCounter.Services;

public class CountLogger : BackgroundService
{
	private readonly HistoryRepository _repo;
	private readonly BookmarkReader _reader;
	private readonly IConfiguration _config;
	private readonly ILogger<CountLogger> _log;

	public CountLogger(HistoryRepository repo, BookmarkReader reader,
					   IConfiguration config, ILogger<CountLogger> log)
	{
		_repo = repo;
		_reader = reader;
		_config = config;
		_log = log;
	}

	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		try
		{
			await LogSafeAsync();
			using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
			while (await timer.WaitForNextTickAsync(ct))
				await LogSafeAsync();
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			// The host is stopping; cancellation of the timer is expected.
		}
	}

	private async Task LogSafeAsync()
	{
		try
		{
			var folder = _config["Bookmarks:Folder"] ?? "YT_Prio";
			var count = _reader.CountFolder(folder);
			if (count >= 0)
			{
				await _repo.InsertAsync(folder, count);
				_log.LogInformation("Logged {Folder} = {Count}", folder, count);
			}
			else
			{
				_log.LogWarning("Bookmarks file not found at {Path}",
					_config["Bookmarks:EdgeProfilePath"]);
			}
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Failed to log count");
		}
	}
}