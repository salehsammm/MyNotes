using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public record CollectionSummary(int Id, string Name, string ArchiveName, int Count);
public record CollectionPage(List<ArchiveItem> Items, int Total);

public class CollectionService(IDbContextFactory<AppDbContext> factory)
{
    public static readonly string[] Kinds = ["Bookmark", "Video", "Search", "Profile", "Board", "Tool", "Image", "Text", "Series"];

    public async Task<List<CollectionSummary>> FoldersAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.ArchiveCollections.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new CollectionSummary(x.Id, x.Name, x.ArchiveName, x.Items.Count)).ToListAsync();
    }

    public async Task<CollectionPage> SearchAsync(int folder, string search, string kind, string status, string priority, int page, int performer = 0)
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = db.ArchiveItems.AsNoTracking().AsQueryable();
        if (folder > 0) query = query.Where(x => x.ArchiveCollectionId == folder);
        if (performer > 0) query = query.Where(x => x.PerformerId == performer);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.SourceNote.Contains(search)
            || x.Notes.Contains(search) || (x.Url != null && x.Url.Contains(search)) || x.Collection.Name.Contains(search));
        if (kind.Length > 0) query = query.Where(x => x.Kind == kind);
        if (status.Length > 0) query = query.Where(x => x.Status.Contains(status));
        if (priority.Length > 0) query = query.Where(x => x.Priority.Contains(priority));
        var total = await query.CountAsync();
        var items = await query.Include(x => x.Scene)
            .OrderBy(x => x.Collection.Name).ThenBy(x => x.SourceRow).Skip(Math.Max(0, page) * 40).Take(40).ToListAsync();
        return new(items, total);
    }

    public async Task<ArchiveItem?> GetAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.ArchiveItems.AsNoTracking().Include(x => x.Scene).SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task SaveAsync(ArchiveItem edit)
    {
        if (!Kinds.Contains(edit.Kind)) throw new InvalidOperationException("Choose a bookmark type.");
        await using var db = await factory.CreateDbContextAsync();
        if (edit.SceneId is int sceneId && !await db.Scenes.AnyAsync(x => x.Id == sceneId))
            throw new InvalidOperationException("That scene ID does not exist.");
        if (edit.PerformerId is int performerId && !await db.Performers.AnyAsync(x => x.Id == performerId))
            throw new InvalidOperationException("That performer ID does not exist.");
        var item = await db.ArchiveItems.SingleAsync(x => x.Id == edit.Id);
        item.Notes = edit.Notes.Trim();
        item.Status = edit.Status.Trim();
        item.Priority = edit.Priority.Trim();
        item.Progress = edit.Progress.Trim();
        item.Kind = edit.Kind;
        item.SceneId = edit.SceneId;
        item.PerformerId = edit.PerformerId;
        await db.SaveChangesAsync();
    }

    public static string? SafeUrl(string? url) => Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == "https" || uri.Scheme == "http") ? url : null;
}
