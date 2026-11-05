using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class PlaylistService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<Playlist>> GetAllAsync(bool includeSceneDetails = true)
    {
        await using var db = await factory.CreateDbContextAsync();
        IQueryable<Playlist> query = db.Playlists.AsNoTracking().Include(p => p.Scenes);
        if (includeSceneDetails) query = query.Include(p => p.Scenes).ThenInclude(p => p.Scene);
        return await query.AsSplitQuery().OrderBy(p => p.Name).ToListAsync();
    }

    private static string ValidName(string name)
    {
        name = name.Trim();
        if (name.Length is < 1 or > 160)
            throw new InvalidOperationException("Enter a playlist name between 1 and 160 characters.");
        return name;
    }

    public async Task<Playlist> CreateAsync(string name, string description = "")
    {
        name = ValidName(name);
        if (description.Length > 10000) throw new InvalidOperationException("Keep playlist descriptions within 10,000 characters.");
        await using var db = await factory.CreateDbContextAsync();
        if (await db.Playlists.AnyAsync(p => p.Name == name))
            throw new InvalidOperationException("A playlist with this name already exists.");
        var playlist = new Playlist { Name = name, Description = description };
        db.Playlists.Add(playlist);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        { throw new InvalidOperationException("A playlist with this name already exists."); }
        return playlist;
    }

    public async Task RenameAsync(int id, string name)
    {
        name = ValidName(name);
        await using var db = await factory.CreateDbContextAsync();
        var playlist = await db.Playlists.FindAsync(id)
            ?? throw new InvalidOperationException("This playlist no longer exists.");
        if (await db.Playlists.AnyAsync(p => p.Id != id && p.Name == name))
            throw new InvalidOperationException("A playlist with this name already exists.");
        playlist.Name = name;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        { throw new InvalidOperationException("A playlist with this name already exists."); }
    }

    public async Task SaveDescriptionAsync(int id, string description)
    {
        if (description.Length > 10000) throw new InvalidOperationException("Keep playlist descriptions within 10,000 characters.");
        await using var db = await factory.CreateDbContextAsync();
        var playlist = await db.Playlists.FindAsync(id)
            ?? throw new InvalidOperationException("This playlist no longer exists.");
        playlist.Description = description;
        await db.SaveChangesAsync();
    }

    private static bool IsDuplicate(DbUpdateException ex) =>
        ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Playlists.Where(p => p.Id == id).ExecuteDeleteAsync();
    }

    // Appends, removals and reorders lock the parent row until their transaction commits.
    // This also serializes max+1 allocation across concurrent tabs and editor saves.
    internal static async Task LockAsync(AppDbContext db, int playlistId)
    {
        if (await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Playlists WITH (ROWLOCK) SET Name = Name WHERE Id = {playlistId}") == 0)
            throw new InvalidOperationException("This playlist no longer exists.");
    }

    private static async Task<int> NextPositionAsync(AppDbContext db, int playlistId) =>
        checked((await db.PlaylistScenes.Where(p => p.PlaylistId == playlistId).MaxAsync(p => (int?)p.Position) ?? 0) + 1);

    public async Task AddSceneAsync(int playlistId, int sceneId)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAsync(db, playlistId);
        if (!await db.Scenes.AnyAsync(s => s.Id == sceneId))
            throw new InvalidOperationException("Choose an existing scene.");
        if (!await db.PlaylistScenes.AnyAsync(p => p.PlaylistId == playlistId && p.SceneId == sceneId))
        {
            db.PlaylistScenes.Add(new PlaylistScene { PlaylistId = playlistId, SceneId = sceneId,
                Position = await NextPositionAsync(db, playlistId) });
            await db.SaveChangesAsync();
        }
        await transaction.CommitAsync();
    }

    public async Task RemoveSceneAsync(int playlistId, int sceneId)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAsync(db, playlistId);
        await db.PlaylistScenes.Where(p => p.PlaylistId == playlistId && p.SceneId == sceneId).ExecuteDeleteAsync();
        await transaction.CommitAsync();
    }

    public async Task MoveSceneAsync(int playlistId, int sceneId, int position)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAsync(db, playlistId);
        var entries = await db.PlaylistScenes.Where(p => p.PlaylistId == playlistId)
            .OrderBy(p => p.Position).ThenBy(p => p.SceneId).ToListAsync();
        var entry = entries.SingleOrDefault(p => p.SceneId == sceneId)
            ?? throw new InvalidOperationException("This scene is no longer in the playlist.");
        if (position < 1 || position > entries.Count)
            throw new InvalidOperationException("Choose a position within the playlist.");
        entries.Remove(entry);
        entries.Insert(position - 1, entry);
        for (var i = 0; i < entries.Count; i++) entries[i].Position = i + 1;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task SaveNoteAsync(int playlistId, int sceneId, string note)
    {
        if (note.Length > 10000) throw new InvalidOperationException("Keep playlist notes within 10,000 characters.");
        await using var db = await factory.CreateDbContextAsync();
        var entry = await db.PlaylistScenes.FindAsync(playlistId, sceneId)
            ?? throw new InvalidOperationException("This scene is no longer in the playlist.");
        // Only the note is changed, so a simultaneous reorder cannot be overwritten.
        entry.Note = note;
        await db.SaveChangesAsync();
    }

    // Called inside the scene save so review edits and membership changes commit together.
    internal static async Task SetMembershipAsync(AppDbContext db, Scene scene, IReadOnlyCollection<int>? playlistIds)
    {
        if (playlistIds is null) return;
        var ids = playlistIds.Distinct().Order().ToArray();
        foreach (var id in ids) await LockAsync(db, id);
        if (await db.Playlists.CountAsync(p => ids.Contains(p.Id)) != ids.Length)
            throw new InvalidOperationException("A selected playlist no longer exists. Reopen the playlist picker and try again.");
        var existing = scene.Id == 0 ? new List<PlaylistScene>() :
            await db.PlaylistScenes.Where(p => p.SceneId == scene.Id).ToListAsync();
        db.PlaylistScenes.RemoveRange(existing.Where(p => !ids.Contains(p.PlaylistId)));
        foreach (var id in ids.Where(id => existing.All(p => p.PlaylistId != id)))
            db.PlaylistScenes.Add(new PlaylistScene { PlaylistId = id, Scene = scene,
                Position = await NextPositionAsync(db, id) });
    }
}
