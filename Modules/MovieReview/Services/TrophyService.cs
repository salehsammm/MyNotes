using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class TrophyService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    public TrophyService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<TrophyCategory>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.TrophyCategories.AsNoTracking()
            .Include(c => c.Entries.OrderBy(e => e.Rank).ThenBy(e => e.Id))
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id).ToListAsync();
    }

    public async Task AddCategoryAsync(string name)
    {
        name = name.Trim();
        if (name.Length == 0) return;
        name = name[..Math.Min(name.Length, 160)];
        await using var db = await _factory.CreateDbContextAsync();
        if (await db.TrophyCategories.AnyAsync(c => c.Name == name)) return;
        var next = (await db.TrophyCategories.MaxAsync(c => (int?)c.SortOrder) ?? 0) + 1;
        db.TrophyCategories.Add(new TrophyCategory { Name = name, SortOrder = next });
        await db.SaveChangesAsync();
    }

    public async Task RenameCategoryAsync(int id, string name)
    {
        name = name.Trim();
        if (name.Length == 0) return;
        name = name[..Math.Min(name.Length, 160)];
        await using var db = await _factory.CreateDbContextAsync();
        if (await db.TrophyCategories.AnyAsync(c => c.Id != id && c.Name == name)) return;
        var category = await db.TrophyCategories.FindAsync(id);
        if (category is null) return;
        category.Name = name;
        await db.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var category = await db.TrophyCategories.FindAsync(id);
        if (category is null) return;
        db.TrophyCategories.Remove(category);
        await db.SaveChangesAsync();
    }

    public async Task SaveEntryAsync(int id, int categoryId, int rank, string description, string? references)
    {
        description = description.Trim();
        if (description.Length == 0) return;
        await using var db = await _factory.CreateDbContextAsync();
        TrophyEntry? entry = id == 0 ? new TrophyEntry() : await db.TrophyEntries.FindAsync(id);
        if (entry is null) return;
        entry.TrophyCategoryId = categoryId;
        entry.Rank = Math.Max(1, rank);
        entry.Description = description;
        entry.References = string.IsNullOrWhiteSpace(references) ? null : references.Trim();
        if (id == 0) db.TrophyEntries.Add(entry);
        await db.SaveChangesAsync();
    }

    public async Task DeleteEntryAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var entry = await db.TrophyEntries.FindAsync(id);
        if (entry is null) return;
        db.TrophyEntries.Remove(entry);
        await db.SaveChangesAsync();
    }

    public async Task<(int Categories, int Entries)> ImportTextAsync(string text)
    {
        var parsed = Parse(text);
        await using var db = await _factory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var existing = await db.TrophyCategories.Include(c => c.Entries).ToListAsync();
        var nextOrder = existing.Select(c => c.SortOrder).DefaultIfEmpty().Max();
        var addedCategories = 0;
        var addedEntries = 0;
        foreach (var source in parsed)
        {
            var category = existing.FirstOrDefault(c => string.Equals(c.Name, source.Name, StringComparison.OrdinalIgnoreCase));
            if (category is null)
            {
                category = new TrophyCategory { Name = source.Name, SortOrder = ++nextOrder };
                db.TrophyCategories.Add(category);
                existing.Add(category);
                addedCategories++;
            }
            foreach (var item in source.Entries)
            {
                if (category.Entries.Any(e => string.Equals(e.Description, item.Description, StringComparison.OrdinalIgnoreCase))) continue;
                category.Entries.Add(new TrophyEntry
                {
                    Rank = item.Rank,
                    Description = item.Description,
                    References = item.References
                });
                addedEntries++;
            }
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return (addedCategories, addedEntries);
    }

    private static List<ParsedCategory> Parse(string text)
    {
        var categories = new List<ParsedCategory>();
        ParsedCategory? current = null;
        ParsedEntry? entry = null;
        foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (line.EndsWith(':') && !Regex.IsMatch(line, @"^\d+\s*-"))
            {
                var name = line[..^1].Trim();
                current = new ParsedCategory(name[..Math.Min(name.Length, 160)]);
                categories.Add(current);
                entry = null;
                continue;
            }
            var rankMatch = Regex.Match(line, @"^(\d+)\s*-\s*(.+)$");
            if (rankMatch.Success && current is not null)
            {
                entry = new ParsedEntry(int.Parse(rankMatch.Groups[1].Value), rankMatch.Groups[2].Value.Trim());
                current.Entries.Add(entry);
                continue;
            }
            if (entry is null) continue;
            if (line.StartsWith('(') && line.EndsWith(')'))
            {
                var value = line[1..^1].Trim();
                if (value.StartsWith("ref:", StringComparison.OrdinalIgnoreCase)) value = value[4..].Trim();
                entry.References = value;
            }
            else entry.Description += " " + line;
        }
        return categories.Where(c => c.Entries.Count > 0).ToList();
    }

    private sealed class ParsedCategory(string name)
    {
        public string Name { get; } = name;
        public List<ParsedEntry> Entries { get; } = new();
    }
    private sealed class ParsedEntry(int rank, string description)
    {
        public int Rank { get; } = rank;
        public string Description { get; set; } = description;
        public string? References { get; set; }
    }
}
