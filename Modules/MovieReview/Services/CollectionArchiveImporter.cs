using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

// Local maintenance only: archive contents never become migrations or repository seed data.
public static class CollectionArchiveImporter
{
    public static async Task ImportAsync(AppDbContext db, string directory)
    {
        var paths = Directory.GetFiles(Path.GetFullPath(directory), "*.zip").Order(StringComparer.OrdinalIgnoreCase).ToArray();
        if (paths.Length == 0) throw new InvalidOperationException("No ZIP archives found.");
        var scenes = await db.Scenes.AsNoTracking().Select(x => new { x.Id, x.RaindropId, x.Url, x.AlternateUrls, x.SourceData }).ToListAsync();
        var ids = new Dictionary<string, HashSet<int>>();
        var urls = new Dictionary<string, HashSet<int>>();
        void Index(Dictionary<string, HashSet<int>> index, string? key, int id)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!index.TryGetValue(key, out var values)) index[key] = values = [];
            values.Add(id);
        }
        foreach (var scene in scenes)
        {
            Index(ids, scene.RaindropId, scene.Id); Index(urls, Normalize(scene.Url), scene.Id);
            foreach (var url in JsonSerializer.Deserialize<string[]>(scene.AlternateUrls ?? "[]") ?? []) Index(urls, Normalize(url), scene.Id);
            foreach (var row in JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(scene.SourceData ?? "[]") ?? [])
            {
                if (row.TryGetValue("id", out var id)) Index(ids, id.ToString(), scene.Id);
                if (row.TryGetValue("url", out var url)) Index(urls, Normalize(url.ToString()), scene.Id);
            }
        }

        var existing = (await db.ArchiveCollections.Select(x => x.Sha256).ToListAsync()).ToHashSet();
        int imported = 0, skipped = 0, bookmarks = 0, texts = 0, linked = 0, ambiguous = 0;
        await using var transaction = await db.Database.BeginTransactionAsync();
        foreach (var path in paths)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            var hash = Convert.ToHexString(SHA256.HashData(bytes));
            if (!existing.Add(hash)) { skipped++; continue; }
            using var zip = new ZipArchive(new MemoryStream(bytes));
            var export = zip.GetEntry("export.csv") ?? throw new InvalidDataException($"Missing export.csv: {Path.GetFileName(path)}");
            var collection = new ArchiveCollection { Name = Path.GetFileNameWithoutExtension(path), ArchiveName = Path.GetFileName(path),
                Sha256 = hash, OriginalArchive = bytes, ImportedUtc = DateTime.UtcNow };
            using var csvReader = new StreamReader(export.Open(), Encoding.UTF8, true);
            using var csv = ReadCsv(await csvReader.ReadToEndAsync()).GetEnumerator();
            if (!csv.MoveNext()) throw new InvalidDataException("Missing CSV header.");
            var headers = csv.Current;
            if (!new[] { "id", "title", "note", "url" }.All(headers.Contains)) throw new InvalidDataException("Unsupported CSV fields.");
            int number = 0;
            while (csv.MoveNext())
            {
                var fields = csv.Current;
                if (fields.Length != headers.Length) throw new InvalidDataException($"CSV column count mismatch: {path}");
                var source = headers.Zip(fields).ToDictionary(x => x.First, x => x.Second);
                var item = new ArchiveItem { SourceRow = ++number, Title = source["title"], Url = source["url"], SourceNote = source["note"],
                    SourceData = JsonSerializer.Serialize(source, new JsonSerializerOptions { WriteIndented = true }), Kind = Classify(source["url"], source["title"]) };
                if (zip.GetEntry(source["title"]) is { } attachment && attachment.Name != "export.csv" && attachment.Name != "export.html" && attachment.Name != "export.txt")
                    item.AttachmentName = attachment.FullName;
                if (Uri.TryCreate(item.Url, UriKind.Absolute, out var uri))
                {
                    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                    item.Progress = string.Join("; ", query.Where(x => x.Key is "page" or "p").Select(x => x.Key + "=" + x.Value));
                }
                var matches = new HashSet<int>();
                if (ids.TryGetValue(source["id"], out var byId)) matches.UnionWith(byId);
                if (urls.TryGetValue(Normalize(item.Url), out var byUrl)) matches.UnionWith(byUrl);
                if (matches.Count == 1) { item.SceneId = matches.Single(); linked++; }
                else if (matches.Count > 1) ambiguous++;
                collection.Items.Add(item); bookmarks++;
            }
            // export.txt/html repeat the CSV; extra text files are distinct archived records.
            foreach (var entry in zip.Entries.Where(x => x.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && x.FullName != "export.txt"))
            {
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8, true);
                collection.Items.Add(new ArchiveItem { SourceRow = ++number, Title = entry.Name, Kind = "Text", AttachmentName = entry.FullName,
                    SourceNote = await reader.ReadToEndAsync(), SourceData = JsonSerializer.Serialize(new { file = entry.FullName }) });
                texts++;
            }
            db.ArchiveCollections.Add(collection);
            imported++;
            // Flush each archive to keep change tracking bounded; the transaction covers the whole batch.
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            Console.WriteLine($"Archived {collection.ArchiveName}: {collection.Items.Count} records.");
        }
        await transaction.CommitAsync();
        Console.WriteLine($"Collections imported: {imported}; already present: {skipped}; bookmarks: {bookmarks}; extra text files: {texts}; scene links: {linked}; ambiguous links held: {ambiguous}.");
    }

    // Preserve embedded blank lines and line endings exactly (TextFieldParser drops blank lines).
    private static IEnumerable<string[]> ReadCsv(string text)
    {
        var row = new List<string>();
        var field = new StringBuilder();
        bool quoted = false, started = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (quoted)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else quoted = false;
                }
                else field.Append(c);
            }
            else if (c == '"' && field.Length == 0 && !started) { quoted = true; started = true; }
            else if (c == ',') { row.Add(field.ToString()); field.Clear(); started = false; }
            else if (c is '\r' or '\n')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                row.Add(field.ToString()); yield return row.ToArray(); row.Clear(); field.Clear(); started = false;
            }
            else { field.Append(c); started = true; }
        }
        if (quoted) throw new InvalidDataException("Unclosed CSV quote.");
        if (row.Count > 0 || field.Length > 0 || started) { row.Add(field.ToString()); yield return row.ToArray(); }
    }

    private static string Normalize(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return value ?? "";
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query)
            .Where(x => !x.Key.StartsWith("utm_", StringComparison.OrdinalIgnoreCase) && x.Key is not "fbclid" and not "gclid")
            .OrderBy(x => x.Key, StringComparer.Ordinal).SelectMany(x => x.Value.Order(StringComparer.Ordinal).Select(v => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(v ?? "")));
        var host = uri.Host.ToLowerInvariant();
        if (host.StartsWith("www.", StringComparison.Ordinal)) host = host[4..];
        return host + (uri.IsDefaultPort ? "" : ":" + uri.Port.ToString(CultureInfo.InvariantCulture))
            + uri.AbsolutePath.TrimEnd('/') + "?" + string.Join("&", query);
    }

    private static string Classify(string url, string title)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return "Bookmark";
        var path = uri.AbsolutePath.ToLowerInvariant();
        if (uri.Host.Equals("api.raindrop.io", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/file") && title.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return "Image";
        if (path.Contains("/search") || Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).ContainsKey("search")) return "Search";
        if (uri.Host.Equals("miniapps.ai", StringComparison.OrdinalIgnoreCase)) return "Tool";
        if (uri.Host.EndsWith("sex.com", StringComparison.OrdinalIgnoreCase) && path.StartsWith("/user/")) return path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length > 2 ? "Board" : "Profile";
        if (path.Contains("/tvshows/")) return "Series";
        if (path.Contains("/view_video.php") || path.Contains("/movies/") || path.Contains("/video/") || path.Contains("/videos/")) return "Video";
        return "Bookmark";
    }
}
