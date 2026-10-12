using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MyNotes.Data;

namespace MyNotes.Components.Pages;

public partial class Home
{
    private ElementReference seasonDialog;
    private SeasonInput seasonForm = new();
    private string? seasonError;
    private bool savingSeason;

    private sealed class SeasonInput
    {
        [Range(1, 999)]
        public int SeasonNumber { get; set; } = 1;

        [Range(1, 9999)]
        public int EpisodeCount { get; set; } = 10;
    }

    private async Task AddSeason()
    {
        if (selectedItem?.Kind != MediaKind.Series) return;
        seasonForm = new()
        {
            SeasonNumber = Math.Min(999, selectedItem.Episodes.Select(x => x.SeasonNumber).DefaultIfEmpty(0).Max() + 1)
        };
        seasonError = null;
        journalModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "/journal.js");
        await journalModule.InvokeVoidAsync("openEpisode", seasonDialog);
    }

    private async Task CloseSeason()
    {
        if (savingSeason) return;
        if (journalModule is not null) await journalModule.InvokeVoidAsync("closeEpisode", seasonDialog);
    }

    private async Task SaveSeason()
    {
        if (selectedId is not int mediaId || savingSeason) return;
        savingSeason = true;
        seasonError = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var parent = await db.MediaItems.FindAsync(mediaId);
            if (parent?.Kind != MediaKind.Series)
            {
                seasonError = "This series is no longer available. Refresh the journal and try again.";
                return;
            }

            var existing = (await db.Episodes.Where(x => x.MediaItemId == mediaId && x.SeasonNumber == seasonForm.SeasonNumber)
                .Select(x => x.EpisodeNumber).ToListAsync()).ToHashSet();
            var missing = Enumerable.Range(1, seasonForm.EpisodeCount).Where(x => !existing.Contains(x)).ToList();
            if (missing.Count == 0)
            {
                seasonError = "All episodes in this range already exist. Nothing was changed.";
                return;
            }

            db.Episodes.AddRange(missing.Select(number => new Episode
            {
                MediaItemId = mediaId,
                SeasonNumber = seasonForm.SeasonNumber,
                EpisodeNumber = number
            }));
            parent.UpdatedAtUtc = DateTime.UtcNow;
            // One save keeps the entire batch atomic, including the parent timestamp.
            await db.SaveChangesAsync();
            await Refresh();
            await journalModule!.InvokeVoidAsync("closeEpisode", seasonDialog);
            error = null;
            var skipped = seasonForm.EpisodeCount - missing.Count;
            message = $"Season {seasonForm.SeasonNumber}: added {missing.Count} episode{(missing.Count == 1 ? "" : "s")}."
                + (skipped > 0 ? $" Kept {skipped} existing episode{(skipped == 1 ? "" : "s")}." : "");
        }
        catch (DbUpdateException)
        {
            seasonError = "Could not add the season. Another change may have been made; try again. No episodes from this batch were saved.";
        }
        catch (Exception ex) { seasonError = $"Could not add season: {ex.Message}"; }
        finally { savingSeason = false; }
    }
}
