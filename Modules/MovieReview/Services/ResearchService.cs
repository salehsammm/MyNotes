using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class ResearchService
{
	private readonly IDbContextFactory<AppDbContext> _factory;

	public ResearchService(IDbContextFactory<AppDbContext> factory)
	{
		_factory = factory;
	}

	public async Task<List<ResearchDoc>> GetDocsAsync()
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.ResearchDocs.OrderBy(d => d.SortOrder).ToListAsync();
	}

	public async Task<ResearchDoc?> GetDocAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.ResearchDocs.FindAsync(id);
	}

	public async Task<List<ResearchSection>> GetSectionsAsync(int docId)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.ResearchSections
			.Where(s => s.ResearchDocId == docId)
			.OrderBy(s => s.SortOrder)
			.ToListAsync();
	}

	public async Task<ResearchDoc> AddDocAsync(string title, string? category)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var max = await db.ResearchDocs.AnyAsync() ? await db.ResearchDocs.MaxAsync(d => d.SortOrder) : 0;
		var doc = new ResearchDoc { Title = title, Category = category, SortOrder = max + 1 };
		db.ResearchDocs.Add(doc);
		await db.SaveChangesAsync();
		return doc;
	}

	public async Task UpdateDocAsync(ResearchDoc doc)
	{
		await using var db = await _factory.CreateDbContextAsync();
		db.ResearchDocs.Update(doc);
		await db.SaveChangesAsync();
	}

	public async Task DeleteDocAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var sections = await db.ResearchSections.Where(s => s.ResearchDocId == id).ToListAsync();
		db.ResearchSections.RemoveRange(sections);
		var doc = await db.ResearchDocs.FindAsync(id);
		if (doc is not null) db.ResearchDocs.Remove(doc);
		await db.SaveChangesAsync();
	}

	public async Task<ResearchSection> AddSectionAsync(int docId, int? parentId, string heading, string? weight, string body)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var max = await db.ResearchSections
			.Where(s => s.ResearchDocId == docId && s.ParentId == parentId)
			.Select(s => (int?)s.SortOrder)
			.MaxAsync() ?? 0;

		var section = new ResearchSection
		{
			ResearchDocId = docId,
			ParentId = parentId,
			Heading = heading,
			Weight = weight,
			Body = body,
			SortOrder = max + 1
		};
		db.ResearchSections.Add(section);
		await db.SaveChangesAsync();
		return section;
	}

	public async Task UpdateSectionAsync(ResearchSection section)
	{
		await using var db = await _factory.CreateDbContextAsync();
		db.ResearchSections.Update(section);
		await db.SaveChangesAsync();
	}

	public async Task DeleteSectionAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();

		// Collect all descendants
		var all = await db.ResearchSections
			.Where(s => s.ResearchDocId == (db.ResearchSections.Where(x => x.Id == id).Select(x => x.ResearchDocId).First()))
			.ToListAsync();

		var toDelete = new List<ResearchSection>();
		void Collect(int sid)
		{
			var s = all.FirstOrDefault(x => x.Id == sid);
			if (s is null) return;
			toDelete.Add(s);
			foreach (var c in all.Where(x => x.ParentId == sid)) Collect(c.Id);
		}
		Collect(id);

		db.ResearchSections.RemoveRange(toDelete);
		await db.SaveChangesAsync();
	}

	public async Task MoveSectionAsync(int id, bool up)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var s = await db.ResearchSections.FindAsync(id);
		if (s is null) return;

		var siblings = await db.ResearchSections
			.Where(x => x.ResearchDocId == s.ResearchDocId && x.ParentId == s.ParentId)
			.OrderBy(x => x.SortOrder)
			.ToListAsync();

		var index = siblings.FindIndex(x => x.Id == id);
		var swapWith = up ? index - 1 : index + 1;
		if (swapWith < 0 || swapWith >= siblings.Count) return;

		(siblings[index].SortOrder, siblings[swapWith].SortOrder) =
			(siblings[swapWith].SortOrder, siblings[index].SortOrder);
		await db.SaveChangesAsync();
	}

	public async Task IndentAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var s = await db.ResearchSections.FindAsync(id);
		if (s is null) return;

		// previous sibling at same level
		var siblings = await db.ResearchSections
			.Where(x => x.ResearchDocId == s.ResearchDocId && x.ParentId == s.ParentId)
			.OrderBy(x => x.SortOrder)
			.ToListAsync();

		var index = siblings.FindIndex(x => x.Id == id);
		if (index <= 0) return;

		var newParent = siblings[index - 1];
		s.ParentId = newParent.Id;

		var max = await db.ResearchSections
			.Where(x => x.ResearchDocId == s.ResearchDocId && x.ParentId == newParent.Id)
			.Select(x => (int?)x.SortOrder)
			.MaxAsync() ?? 0;

		s.SortOrder = max + 1;
		await db.SaveChangesAsync();
	}

	public async Task OutdentAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var s = await db.ResearchSections.FindAsync(id);
		if (s is null || s.ParentId is null) return;

		var oldParent = await db.ResearchSections.FindAsync(s.ParentId.Value);
		if (oldParent is null) return;

		s.ParentId = oldParent.ParentId;

		var max = await db.ResearchSections
			.Where(x => x.ResearchDocId == s.ResearchDocId && x.ParentId == oldParent.ParentId)
			.Select(x => (int?)x.SortOrder)
			.MaxAsync() ?? 0;

		s.SortOrder = max + 1;
		await db.SaveChangesAsync();
	}
}