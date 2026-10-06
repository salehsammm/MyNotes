using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class PerformerService
{
	private readonly IDbContextFactory<AppDbContext> _factory;

	public PerformerService(IDbContextFactory<AppDbContext> factory)
	{
		_factory = factory;
	}

	public async Task<List<Performer>> GetAllAsync(bool includeArchived = false)
	{
		if (!includeArchived) return (await GetPageAsync("", "name", true, 1, int.MaxValue)).Items;
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Performers.AsNoTracking().Include(p => p.SkinColor).Include(p => p.BoobSize)
			.Include(p => p.AssSize).Include(p => p.BjTags).OrderBy(p => p.Name).ToListAsync();
	}

	public async Task<(List<Performer> Items, int Total)> GetPageAsync(string search, string sortKey, bool ascending, int page, int pageSize, bool archived = false)
	{
		await using var db = await _factory.CreateDbContextAsync();
		IQueryable<Performer> query = db.Performers.AsNoTracking().Where(p => p.IsArchived == archived);
		if (!string.IsNullOrWhiteSpace(search))
		{
			var term = search.Trim();
			query = query.Where(p => p.Name.Contains(term) ||
				(p.Notes != null && p.Notes.Contains(term)) ||
				(p.Moans != null && p.Moans.Contains(term)) ||
				(p.BoobsQuality != null && p.BoobsQuality.Contains(term)) ||
				(p.AssQuality != null && p.AssQuality.Contains(term)) ||
				(p.BjQuality != null && p.BjQuality.Contains(term)) ||
				(p.FaceQuality != null && p.FaceQuality.Contains(term)) ||
				(p.BodySize != null && p.BodySize.Contains(term)) ||
				(p.BodyQuality != null && p.BodyQuality.Contains(term)) ||
				(p.LegQuality != null && p.LegQuality.Contains(term)) ||
				(p.SkinColor != null && p.SkinColor.Name.Contains(term)) ||
				(p.BoobSize != null && p.BoobSize.Name.Contains(term)) ||
				(p.AssSize != null && p.AssSize.Name.Contains(term)) ||
				p.BjTags.Any(t => t.Name.Contains(term)));
		}
		var total = await query.CountAsync();
		query = (sortKey, ascending) switch
		{
			("skin", true) => query.OrderBy(p => p.SkinColor!.Name).ThenBy(p => p.Id),
			("skin", false) => query.OrderByDescending(p => p.SkinColor!.Name).ThenBy(p => p.Id),
			("boobs", true) => query.OrderBy(p => p.BoobSize!.Name).ThenBy(p => p.Id),
			("boobs", false) => query.OrderByDescending(p => p.BoobSize!.Name).ThenBy(p => p.Id),
			("ass", true) => query.OrderBy(p => p.AssSize!.Name).ThenBy(p => p.Id),
			("ass", false) => query.OrderByDescending(p => p.AssSize!.Name).ThenBy(p => p.Id),
			("bj", true) => query.OrderBy(p => p.BjQuality).ThenBy(p => p.Id),
			("bj", false) => query.OrderByDescending(p => p.BjQuality).ThenBy(p => p.Id),
			("moans", true) => query.OrderBy(p => p.Moans).ThenBy(p => p.Id),
			("moans", false) => query.OrderByDescending(p => p.Moans).ThenBy(p => p.Id),
			("watchlist", true) => query.OrderBy(p => p.IsWatchlisted).ThenBy(p => p.Id),
			("watchlist", false) => query.OrderByDescending(p => p.IsWatchlisted).ThenBy(p => p.Id),
			("created", true) => query.OrderBy(p => p.CreatedDate).ThenBy(p => p.Id),
			("created", false) => query.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Id),
			("updated", true) => query.OrderBy(p => p.LastUpdate).ThenBy(p => p.Id),
			("updated", false) => query.OrderByDescending(p => p.LastUpdate).ThenBy(p => p.Id),
			("name", false) => query.OrderByDescending(p => p.Name).ThenBy(p => p.Id),
			_ => query.OrderBy(p => p.Name).ThenBy(p => p.Id)
		};
		var ids = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(p => p.Id).ToListAsync();
		if (ids.Count == 0) return (new List<Performer>(), total);
		var items = await db.Performers.AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync();
		var skinIds = items.Where(p => p.SkinColorId.HasValue).Select(p => p.SkinColorId!.Value).Distinct().ToList();
		var boobIds = items.Where(p => p.BoobSizeId.HasValue).Select(p => p.BoobSizeId!.Value).Distinct().ToList();
		var assIds = items.Where(p => p.AssSizeId.HasValue).Select(p => p.AssSizeId!.Value).Distinct().ToList();
		var skins = await db.SkinColors.AsNoTracking().Where(x => skinIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
		var boobs = await db.BoobSizes.AsNoTracking().Where(x => boobIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
		var asses = await db.AssSizes.AsNoTracking().Where(x => assIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
		var tags = await db.Performers.AsNoTracking().Where(p => ids.Contains(p.Id))
			.SelectMany(p => p.BjTags.Select(t => new { PerformerId = p.Id, Tag = t })).ToListAsync();
		foreach (var performer in items)
		{
			if (performer.SkinColorId is int skinId && skins.TryGetValue(skinId, out var skin)) performer.SkinColor = skin;
			if (performer.BoobSizeId is int boobId && boobs.TryGetValue(boobId, out var boob)) performer.BoobSize = boob;
			if (performer.AssSizeId is int assId && asses.TryGetValue(assId, out var ass)) performer.AssSize = ass;
			performer.BjTags = tags.Where(x => x.PerformerId == performer.Id).Select(x => x.Tag).ToList();
		}
		items = ids.Select(id => items.First(p => p.Id == id)).ToList();
		return (items, total);
	}

	public async Task<Performer?> GetAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Performers
			.AsNoTracking()
			.AsSplitQuery()
			.Include(p => p.SkinColor)
			.Include(p => p.BoobSize)
			.Include(p => p.AssSize)
			.Include(p => p.BjTags)
			.FirstOrDefaultAsync(p => p.Id == id);
	}

	public async Task<Performer> AddAsync(Performer p)
	{
		await using var db = await _factory.CreateDbContextAsync();
		await ValidateNameAsync(db, p.Name, 0);
		p.Name = p.Name.Trim();

		var tagIds = p.BjTags.Select(t => t.Id).ToList();
		p.BjTags.Clear();
		var tags = await db.BjTags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
		foreach (var t in tags) p.BjTags.Add(t);

		var now = DateTime.UtcNow;
		p.CreatedDate = now;
		p.LastUpdate = now;

		db.Performers.Add(p);
		await db.SaveChangesAsync();
		return p;
	}

	public async Task UpdateAsync(Performer p)
	{
		await using var db = await _factory.CreateDbContextAsync();
		await ValidateNameAsync(db, p.Name, p.Id);
		p.Name = p.Name.Trim();

		var existing = await db.Performers
			.Include(x => x.BjTags)
			.FirstAsync(x => x.Id == p.Id);

		existing.Name = p.Name;
		existing.SkinColorId = p.SkinColorId;
		existing.BoobSizeId = p.BoobSizeId;
		existing.AssSizeId = p.AssSizeId;
		existing.BoobsQuality = p.BoobsQuality;
		existing.AssQuality = p.AssQuality;
		existing.BjQuality = p.BjQuality;
		existing.Moans = p.Moans;
		existing.Notes = p.Notes;
		existing.FaceQuality = p.FaceQuality;
		existing.BodySize = p.BodySize;
		existing.BodyQuality = p.BodyQuality;
		existing.BellyQuality = p.BellyQuality;
		existing.LegQuality = p.LegQuality;
		existing.DoesNotSpeakEnglish = p.DoesNotSpeakEnglish;
		existing.AgeCategory = p.AgeCategory;
		existing.IsWatchlisted = p.IsWatchlisted;
		existing.LastUpdate = DateTime.UtcNow;

		existing.BjTags.Clear();
		var tagIds = p.BjTags.Select(t => t.Id).ToList();
		var tags = await db.BjTags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
		foreach (var t in tags) existing.BjTags.Add(t);

		await db.SaveChangesAsync();
	}

	private static async Task ValidateNameAsync(AppDbContext db, string? name, int currentId)
	{
		var normalized = name?.Trim();
		if (string.IsNullOrWhiteSpace(normalized))
			throw new InvalidOperationException("Enter a performer name before saving.");
		var comparison = normalized.ToUpper();
		if (await db.Performers.AnyAsync(p => p.Id != currentId && p.Name.Trim().ToUpper() == comparison))
			throw new InvalidOperationException($"A performer named '{normalized}' already exists. Find that performer or merge the existing duplicates.");
	}

	public async Task SetArchivedAsync(int id, bool archived)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var performer = await db.Performers.FindAsync(id) ?? throw new InvalidOperationException("Performer not found.");
		performer.IsArchived = archived;
		performer.LastUpdate = DateTime.UtcNow;
		await db.SaveChangesAsync();
	}

	public async Task MergeAsync(int sourceId, int mainId)
	{
		if (sourceId == mainId) throw new InvalidOperationException("Choose two different performers.");
		await using var db = await _factory.CreateDbContextAsync();
		await using var transaction = await db.Database.BeginTransactionAsync();
		var main = await db.Performers.FindAsync(mainId) ?? throw new InvalidOperationException("Main performer not found.");
		var source = await db.Performers.FindAsync(sourceId) ?? throw new InvalidOperationException("Duplicate performer not found.");
		if (main.IsArchived) throw new InvalidOperationException("Choose an active performer as the main performer.");
		if (source.IsArchived) throw new InvalidOperationException("This performer is already archived.");

		var mainSceneIds = await db.ScenePerformers.Where(sp => sp.PerformerId == mainId).Select(sp => sp.SceneId).ToHashSetAsync();
		var sourceLinks = await db.ScenePerformers.Where(sp => sp.PerformerId == sourceId).ToListAsync();
		foreach (var link in sourceLinks)
		{
			if (mainSceneIds.Add(link.SceneId)) link.PerformerId = mainId;
			else db.ScenePerformers.Remove(link);
		}
		var sceneIds = sourceLinks.Select(sp => sp.SceneId).Distinct().ToList();
		var now = DateTime.UtcNow;
		foreach (var scene in await db.Scenes.Where(s => sceneIds.Contains(s.Id)).ToListAsync()) scene.LastUpdate = now;
		main.LastUpdate = now;
		source.IsArchived = true;
		source.LastUpdate = now;
		await db.SaveChangesAsync();
		await transaction.CommitAsync();
	}

	public async Task<(int ToMove, int AlreadyShared)> GetMergePreviewAsync(int sourceId, int mainId)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var sourceSceneIds = await db.ScenePerformers.Where(sp => sp.PerformerId == sourceId).Select(sp => sp.SceneId).Distinct().ToListAsync();
		var shared = await db.ScenePerformers.CountAsync(sp => sp.PerformerId == mainId && sourceSceneIds.Contains(sp.SceneId));
		return (sourceSceneIds.Count - shared, shared);
	}

	public async Task DeleteAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var p = await db.Performers.FindAsync(id);
		if (p is null) return;
		db.Performers.Remove(p);
		await db.SaveChangesAsync();
	}
}
