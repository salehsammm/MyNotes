using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class NoteService
{
	private readonly IDbContextFactory<AppDbContext> _factory;

	public NoteService(IDbContextFactory<AppDbContext> factory)
	{
		_factory = factory;
	}

	public async Task<List<Note>> GetAllAsync()
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Notes.OrderBy(n => n.SortOrder).ToListAsync();
	}

	public async Task<Note?> GetAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Notes.FindAsync(id);
	}

	public async Task<Note> AddAsync(Note n)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var max = await db.Notes.AnyAsync() ? await db.Notes.MaxAsync(x => x.SortOrder) : 0;
		n.SortOrder = max + 1;
		db.Notes.Add(n);
		await db.SaveChangesAsync();
		return n;
	}

	public async Task UpdateAsync(Note n)
	{
		await using var db = await _factory.CreateDbContextAsync();
		db.Notes.Update(n);
		await db.SaveChangesAsync();
	}

	public async Task DeleteAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var n = await db.Notes.FindAsync(id);
		if (n is null) return;
		db.Notes.Remove(n);
		await db.SaveChangesAsync();
	}

	public async Task MoveAsync(int id, bool up)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var all = await db.Notes.OrderBy(n => n.SortOrder).ToListAsync();
		var i = all.FindIndex(n => n.Id == id);
		if (i < 0) return;
		var swapWith = up ? i - 1 : i + 1;
		if (swapWith < 0 || swapWith >= all.Count) return;
		(all[i].SortOrder, all[swapWith].SortOrder) = (all[swapWith].SortOrder, all[i].SortOrder);
		await db.SaveChangesAsync();
	}
}