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

    public async Task<Playlist> CreateAsync(string name)
    {
        name = ValidName(name);
        await using var db = await factory.CreateDbContextAsync();
        if (await db.Playlists.AnyAsync(p => p.Name == name))
            throw new InvalidOperationException("A playlist with this name already exists.");
        var playlist = new Playlist { Name = name };
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

    private static bool IsDuplicate(DbUpdateException ex) =>
        ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Playlists.Where(p => p.Id == id).ExecuteDeleteAsync();
    }

    public async Task AddSceneAsync(int playlistId, int sceneId)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (!await db.Playlists.AnyAsync(p => p.Id == playlistId) || !await db.Scenes.AnyAsync(s => s.Id == sceneId))
            throw new InvalidOperationException("Choose an existing playlist and scene.");
        if (await db.PlaylistScenes.AnyAsync(p => p.PlaylistId == playlistId && p.SceneId == sceneId)) return;
        db.PlaylistScenes.Add(new PlaylistScene { PlaylistId = playlistId, SceneId = sceneId });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (IsDuplicate(ex)) { /* Already added by another tab. */ }
    }

    public async Task RemoveSceneAsync(int playlistId, int sceneId)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.PlaylistScenes.Where(p => p.PlaylistId == playlistId && p.SceneId == sceneId).ExecuteDeleteAsync();
    }

    // Called inside the scene save so review edits and membership changes commit together.
    internal static async Task SetMembershipAsync(AppDbContext db, Scene scene, IReadOnlyCollection<int>? playlistIds)
    {
        if (playlistIds is null) return;
        var ids = playlistIds.Distinct().ToArray();
        if (await db.Playlists.CountAsync(p => ids.Contains(p.Id)) != ids.Length)
            throw new InvalidOperationException("A selected playlist no longer exists. Reopen the playlist picker and try again.");
        var existing = scene.Id == 0 ? new List<PlaylistScene>() :
            await db.PlaylistScenes.Where(p => p.SceneId == scene.Id).ToListAsync();
        db.PlaylistScenes.RemoveRange(existing.Where(p => !ids.Contains(p.PlaylistId)));
        foreach (var id in ids.Where(id => existing.All(p => p.PlaylistId != id)))
            db.PlaylistScenes.Add(new PlaylistScene { PlaylistId = id, Scene = scene });
    }
}
