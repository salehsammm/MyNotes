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
		return await db.Scenes.AsNoTracking().AsSplitQuery()
			.Include(s => s.Performers).ThenInclude(sp => sp.Performer).ThenInclude(p => p.SkinColor)
			.Include(s => s.Performers).ThenInclude(sp => sp.Performer).ThenInclude(p => p.BoobSize)
			.Include(s => s.Performers).ThenInclude(sp => sp.Performer).ThenInclude(p => p.AssSize)
			.Include(s => s.Performers).ThenInclude(sp => sp.Performer).ThenInclude(p => p.BjTags)
			.FirstOrDefaultAsync(s => s.Id == id);
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

		// Persist a separate graph so a failed save keeps the editor's IDs and links intact.
		var saved = new Scene();
		db.Entry(saved).CurrentValues.SetValues(s);
		var now = DateTime.UtcNow;
		saved.CreatedDate = now;
		saved.LastUpdate = now;
		saved.IsPinnedDraft = s.IsDraft && s.IsPinnedDraft;
		saved.Performers = links;
		db.Scenes.Add(saved);
		await db.SaveChangesAsync();
		s.Id = saved.Id;
		s.CreatedDate = saved.CreatedDate;
		s.LastUpdate = saved.LastUpdate;
		s.IsPinnedDraft = saved.IsPinnedDraft;

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
		existing.SetupWeird = s.SetupWeird;
		existing.SetupDumb = s.SetupDumb;
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
