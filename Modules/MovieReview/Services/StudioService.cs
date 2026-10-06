using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class StudioService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public StudioService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<Studio>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Studios.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<Studio> AddAsync(string? name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Enter a studio name.");
        if (trimmed.Length > 450)
            throw new InvalidOperationException("Studio names must be 450 characters or fewer.");

        await using var db = await _factory.CreateDbContextAsync();
        var normalized = trimmed.ToUpper();
        var existing = await db.Studios.FirstOrDefaultAsync(s => s.Name.ToUpper() == normalized);
        if (existing is not null) return existing;

        var studio = new Studio { Name = trimmed };
        db.Studios.Add(studio);
        await db.SaveChangesAsync();
        return studio;
    }
}
