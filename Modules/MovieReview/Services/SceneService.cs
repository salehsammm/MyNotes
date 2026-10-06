using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class SceneService
{
	private readonly IDbContextFactory<AppDbContext> _factory;

	public SceneService(IDbContextFactory<AppDbContext> factory)
	{
		_factory = factory;
	}

	public async Task<List<Scene>> GetAllAsync()
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Scenes
			.AsNoTracking()
			.AsSplitQuery()
			.Include(s => s.Performers).ThenInclude(sp => sp.Performer)
			.OrderBy(s => s.Title)
			.ToListAsync();
	}

	public async Task<Scene?> GetAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var scene = await db.Scenes.FirstOrDefaultAsync(s => s.Id == id);
		if (scene is null) return null;

		scene.Performers = await db.ScenePerformers
			.Where(sp => sp.SceneId == id)
			.Include(sp => sp.Performer)
			.ToListAsync();

		return scene;
	}

	public async Task<Scene> AddAsync(Scene s)
	{
		await using var db = await _factory.CreateDbContextAsync();

		var links = s.Performers.Select(sp => new ScenePerformer
		{
			PerformerId = sp.PerformerId,
			SkinOverride = sp.SkinOverride,
			BoobsOverride = sp.BoobsOverride,
			AssOverride = sp.AssOverride,
			BjOverride = sp.BjOverride,
			MoansOverride = sp.MoansOverride,
			Notes = sp.Notes
		}).ToList();

		s.Performers.Clear();

		var now = DateTime.UtcNow;
		s.CreatedDate = now;
		s.LastUpdate = now;
		s.IsPinnedDraft = s.IsDraft && s.IsPinnedDraft;

		db.Scenes.Add(s);
		await db.SaveChangesAsync();

		foreach (var l in links)
		{
			l.SceneId = s.Id;
			db.ScenePerformers.Add(l);
		}
		await db.SaveChangesAsync();

		return s;
	}

	public async Task UpdateAsync(Scene s)
	{
		await using var db = await _factory.CreateDbContextAsync();

		var existing = await db.Scenes.FirstAsync(x => x.Id == s.Id);

		existing.Title = s.Title;
		existing.Studio = s.Studio;
		existing.Url = s.Url;
		existing.IsDraft = s.IsDraft;
		existing.IsPinnedDraft = s.IsDraft && existing.IsPinnedDraft;
		existing.ResumeAtSeconds = s.ResumeAtSeconds;
		existing.OverallRating = s.OverallRating;
		existing.OverallModifier = s.OverallModifier;
		existing.Verdict = s.Verdict;
		existing.SetupRating = s.SetupRating;
		existing.SetupModifier = s.SetupModifier;
		existing.SetupNotes = s.SetupNotes;
		existing.SetupSpeaksNoEnglish = s.SetupSpeaksNoEnglish;
		existing.SexRating = s.SexRating;
		existing.SexModifier = s.SexModifier;
		existing.SexNotes = s.SexNotes;
		existing.IsPov = s.IsPov;
		existing.Tags = s.Tags;
		existing.Positions = s.Positions;
		existing.Locations = s.Locations;
		existing.LastUpdate = DateTime.UtcNow;

		var oldLinks = await db.ScenePerformers.Where(sp => sp.SceneId == s.Id).ToListAsync();
		db.ScenePerformers.RemoveRange(oldLinks);
		await db.SaveChangesAsync();

		foreach (var sp in s.Performers)
		{
			db.ScenePerformers.Add(new ScenePerformer
			{
				SceneId = s.Id,
				PerformerId = sp.PerformerId,
				SkinOverride = sp.SkinOverride,
				BoobsOverride = sp.BoobsOverride,
				AssOverride = sp.AssOverride,
				BjOverride = sp.BjOverride,
				MoansOverride = sp.MoansOverride,
				Notes = sp.Notes
			});
		}
		await db.SaveChangesAsync();
	}

	public async Task ToggleDraftPinAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var scene = await db.Scenes.FindAsync(id);
		if (scene is null || !scene.IsDraft) return;
		scene.IsPinnedDraft = !scene.IsPinnedDraft;
		await db.SaveChangesAsync();
	}

	public async Task DeleteAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var links = await db.ScenePerformers.Where(sp => sp.SceneId == id).ToListAsync();
		db.ScenePerformers.RemoveRange(links);
		var s = await db.Scenes.FindAsync(id);
		if (s is not null) db.Scenes.Remove(s);
		await db.SaveChangesAsync();
	}
}
