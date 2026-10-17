using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;
using MyNotes.Security;

namespace MyNotes.Modules.MovieReview;

public static class CollectionEndpoints
{
    public static void MapCollectionArchives(this WebApplication app)
    {
        app.MapGet("/moviereview/collections/{id:int}/archive", async Task<IResult> (int id, HttpContext context, PrivateAccessStore access, DbContextOptions<AppDbContext> options) =>
        {
            // HTTP downloads use the live session directly; the circuit factory belongs to Blazor.
            if (!access.IsUnlocked(context.User)) return Results.Unauthorized();
            await using var db = new AppDbContext(options);
            var archive = await db.ArchiveCollections.AsNoTracking().Where(x => x.Id == id)
                .Select(x => new { x.OriginalArchive, x.ArchiveName }).SingleOrDefaultAsync();
            return archive is null ? Results.NotFound() : Results.File(archive.OriginalArchive, "application/zip", Path.GetFileName(archive.ArchiveName));
        }).RequireAuthorization("PrivateArea");

        app.MapGet("/moviereview/collections/items/{id:int}/file", async Task<IResult> (int id, HttpContext context, PrivateAccessStore access, DbContextOptions<AppDbContext> options) =>
        {
            if (!access.IsUnlocked(context.User)) return Results.Unauthorized();
            await using var db = new AppDbContext(options);
            var item = await db.ArchiveItems.AsNoTracking().Where(x => x.Id == id && x.AttachmentName != null)
                .Select(x => new { x.AttachmentName, x.Collection.OriginalArchive }).SingleOrDefaultAsync();
            if (item is null) return Results.NotFound();
            using var zip = new ZipArchive(new MemoryStream(item.OriginalArchive));
            var entry = zip.GetEntry(item.AttachmentName!);
            if (entry is null) return Results.NotFound();
            using var buffer = new MemoryStream();
            await using var stream = entry.Open();
            await stream.CopyToAsync(buffer);
            var type = Path.GetExtension(entry.Name).ToLowerInvariant() switch { ".png" => "image/png", ".txt" => "text/plain; charset=utf-8", _ => "application/octet-stream" };
            return Results.File(buffer.ToArray(), type, Path.GetFileName(entry.Name));
        }).RequireAuthorization("PrivateArea");
    }
}
