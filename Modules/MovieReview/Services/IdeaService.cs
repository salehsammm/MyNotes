using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class IdeaService
{
	private readonly IDbContextFactory<AppDbContext> _factory;

	public IdeaService(IDbContextFactory<AppDbContext> factory)
	{
		_factory = factory;
	}

	public async Task<List<Idea>> GetAllAsync()
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Ideas
			.OrderBy(i => i.IsDone)
			.ThenBy(i => i.SortOrder)
			.ToListAsync();
	}

	public async Task AddAsync(string text)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var maxOrder = await db.Ideas.AnyAsync() ? await db.Ideas.MaxAsync(i => i.SortOrder) : 0;
		db.Ideas.Add(new Idea { Text = text, SortOrder = maxOrder + 1 });
		await db.SaveChangesAsync();
	}

	public async Task UpdateAsync(Idea idea)
	{
		await using var db = await _factory.CreateDbContextAsync();
		db.Ideas.Update(idea);
		await db.SaveChangesAsync();
	}

	public async Task ToggleDoneAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var i = await db.Ideas.FindAsync(id);
		if (i is null) return;
		i.IsDone = !i.IsDone;
		await db.SaveChangesAsync();
	}

	public async Task DeleteAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var i = await db.Ideas.FindAsync(id);
		if (i is null) return;
		db.Ideas.Remove(i);
		await db.SaveChangesAsync();
	}

	public async Task MoveAsync(int id, bool up)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var all = await db.Ideas.OrderBy(i => i.SortOrder).ToListAsync();
		var index = all.FindIndex(i => i.Id == id);
		if (index < 0) return;
		var swapWith = up ? index - 1 : index + 1;
		if (swapWith < 0 || swapWith >= all.Count) return;
		(all[index].SortOrder, all[swapWith].SortOrder) = (all[swapWith].SortOrder, all[index].SortOrder);
		await db.SaveChangesAsync();
	}
}