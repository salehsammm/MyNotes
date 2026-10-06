using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class LookupService
{
	private readonly IDbContextFactory<AppDbContext> _factory;

	public LookupService(IDbContextFactory<AppDbContext> factory)
	{
		_factory = factory;
	}

	public async Task<List<SkinColor>> GetSkinColorsAsync() =>
		await GetAsync<SkinColor>();

	public async Task<List<BoobSize>> GetBoobSizesAsync() =>
		await GetAsync<BoobSize>();

	public async Task<List<AssSize>> GetAssSizesAsync() =>
		await GetAsync<AssSize>();

	public async Task<List<BjTag>> GetBjTagsAsync() =>
		await GetAsync<BjTag>();

	private async Task<List<T>> GetAsync<T>() where T : class
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Set<T>()
			.Where(e => EF.Property<bool>(e, "IsDeleted") == false)
			.OrderBy(e => EF.Property<string>(e, "Name"))
			.ToListAsync();
	}

	public async Task AddSkinColorAsync(string name) => await AddAsync<SkinColor>(name);
	public async Task AddBoobSizeAsync(string name) => await AddAsync<BoobSize>(name);
	public async Task AddAssSizeAsync(string name) => await AddAsync<AssSize>(name);
	public async Task AddBjTagAsync(string name) => await AddAsync<BjTag>(name);

	private async Task AddAsync<T>(string name) where T : class, new()
	{
		await using var db = await _factory.CreateDbContextAsync();
		var entity = new T();
		entity.GetType().GetProperty("Name")!.SetValue(entity, name);
		entity.GetType().GetProperty("IsDeleted")!.SetValue(entity, false);
		db.Set<T>().Add(entity);
		await db.SaveChangesAsync();
	}

	public async Task SoftDeleteSkinColorAsync(int id) => await SoftDeleteAsync<SkinColor>(id);
	public async Task SoftDeleteBoobSizeAsync(int id) => await SoftDeleteAsync<BoobSize>(id);
	public async Task SoftDeleteAssSizeAsync(int id) => await SoftDeleteAsync<AssSize>(id);
	public async Task SoftDeleteBjTagAsync(int id) => await SoftDeleteAsync<BjTag>(id);

	private async Task SoftDeleteAsync<T>(int id) where T : class
	{
		await using var db = await _factory.CreateDbContextAsync();
		var entity = await db.Set<T>().FindAsync(id);
		if (entity is null) return;
		entity.GetType().GetProperty("IsDeleted")!.SetValue(entity, true);
		await db.SaveChangesAsync();
	}

	public async Task<bool> IsSkinColorInUseAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Performers.AnyAsync(p => p.SkinColorId == id);
	}

	public async Task<bool> IsBoobSizeInUseAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Performers.AnyAsync(p => p.BoobSizeId == id);
	}

	public async Task<bool> IsAssSizeInUseAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Performers.AnyAsync(p => p.AssSizeId == id);
	}

	public async Task<bool> IsBjTagInUseAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Performers.AnyAsync(p => p.BjTags.Any(t => t.Id == id));
	}
}