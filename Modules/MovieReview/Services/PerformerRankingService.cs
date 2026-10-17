using System.Data;
using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public record PerformerStanding(int Id, string Name, int? Position, bool MyGs, bool Archived);
public class PerformerRankingService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<PerformerStanding>> GetAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Performers.AsNoTracking().OrderBy(x => x.RankingPosition == null).ThenBy(x => x.RankingPosition).ThenBy(x => x.Name)
            .Select(x => new PerformerStanding(x.Id, x.Name, x.RankingPosition,
                db.ArchiveItems.Any(i => i.PerformerId == x.Id && i.Collection.ArchiveName == "My_Gs.zip"), x.IsArchived)).ToListAsync();
    }

    public async Task MoveAsync(int performerId, int? requestedPosition)
    {
        if (requestedPosition < 1) throw new InvalidOperationException("Position must be at least 1.");
        await using var db = await factory.CreateDbContextAsync();
        // Serialize reorder operations so two browser tabs cannot leave duplicate positions or gaps.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var ranked = await db.Performers.Where(x => x.RankingPosition != null).OrderBy(x => x.RankingPosition).ThenBy(x => x.Id).ToListAsync();
        var performer = ranked.FirstOrDefault(x => x.Id == performerId) ?? await db.Performers.SingleAsync(x => x.Id == performerId);
        ranked.Remove(performer);
        performer.RankingPosition = null;
        if (requestedPosition is int position) ranked.Insert(Math.Min(position - 1, ranked.Count), performer);
        for (int i = 0; i < ranked.Count; i++) ranked[i].RankingPosition = i + 1;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public static async Task LinkMyGsAsync(AppDbContext db)
    {
        var items = await db.ArchiveItems.Include(x => x.Collection).Where(x => x.Collection.ArchiveName == "My_Gs.zip").ToListAsync();
        var performers = await db.Performers.ToListAsync();
        int created = 0, linked = 0;
        await using var transaction = await db.Database.BeginTransactionAsync();
        foreach (var item in items)
        {
            if (item.PerformerId is not null) continue;
            var name = System.Text.RegularExpressions.Regex.Split(item.Title, @" Nude XXX Pics| Porn Pics| nude", System.Text.RegularExpressions.RegexOptions.IgnoreCase)[0].Trim();
            if (name.Length == 0 || name == item.Title) throw new InvalidDataException($"Review performer label for archive row {item.SourceRow}.");
            var matches = performers.Where(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count > 1) throw new InvalidOperationException($"Multiple performer profiles match archive row {item.SourceRow}.");
            var performer = matches.SingleOrDefault();
            if (performer is null)
            {
                performer = new Performer { Name = name, CreatedDate = DateTime.UtcNow, LastUpdate = DateTime.UtcNow };
                db.Performers.Add(performer); performers.Add(performer); created++;
            }
            item.Performer = performer;
            item.Kind = "Profile";
            linked++;
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        Console.WriteLine($"My_Gs: {linked} archive links saved; {created} performer profiles added. All ranking positions remain owner-controlled.");
    }
}
