using Microsoft.EntityFrameworkCore;
using MyNotes.Data;

namespace MyNotes.Components.Pages;

public partial class PersonalNotes
{
    private bool canReadLocked;
    private bool selectedNoteLocked;
    private readonly CancellationTokenSource privacyCancellation = new();
    private Task? privacyWatch;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) privacyWatch = WatchPrivacy();
        return Task.CompletedTask;
    }

    private async Task WatchPrivacy()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(privacyCancellation.Token))
                await InvokeAsync(async () =>
                {
                    try
                    {
                        var previouslyUnlocked = canReadLocked;
                        var previousSelection = selectedId;
                        var previousCount = notes.Count;
                        await RefreshAccess();
                        if (previouslyUnlocked != canReadLocked || previousSelection != selectedId || previousCount != notes.Count)
                        {
                            if (!busy) await Run(RefreshResults);
                            StateHasChanged();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Could not refresh note privacy state");
                        StateHasChanged();
                    }
                });
        }
        catch (OperationCanceledException) when (privacyCancellation.IsCancellationRequested) { }
    }

    private async Task RefreshAccess()
    {
        var previouslyUnlocked = canReadLocked;
        var state = await AuthenticationState.GetAuthenticationStateAsync();
        canReadLocked = PrivateAccess.IsUnlocked(state.User);
        if (canReadLocked) return;
        notes.RemoveAll(n => n.IsLocked);
        ClearProtectedEditor();
        if (previouslyUnlocked) { categories.Clear(); groupCounts.Clear(); resultCount = notes.Count; }
        var visibleIds = notes.Select(n => n.Id).ToList();
        if (selectedId is int currentId) visibleIds.Add(currentId);
        if (visibleIds.Count > 0)
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var protectedIds = await db.PersonalNotes.Where(n => n.IsLocked && visibleIds.Contains(n.Id)).Select(n => n.Id).ToListAsync();
            if (selectedId is int id && protectedIds.Contains(id)) selectedNoteLocked = true;
            if (notes.RemoveAll(n => protectedIds.Contains(n.Id)) > 0) previouslyUnlocked = true;
        }
        if (previouslyUnlocked) { categories.Clear(); groupCounts.Clear(); resultCount = notes.Count; }
        ClearProtectedEditor();
    }

    private void ClearProtectedEditor()
    {
        if (selectedNoteLocked)
        {
            selectedId = null;
            SetForm(new() { NoteDate = Today });
            message = "Private session locked. Unlock locked notes to continue.";
        }
    }

    // Apply before search, projection or loading research so private text never reaches public results.
    private IQueryable<PersonalNote> VisibleNotes(MediaDbContext db)
        => db.PersonalNotes.Where(n => !n.IsLocked || canReadLocked);

    private async Task<bool> PrivateSessionIsLive()
        => PrivateAccess.IsUnlocked((await AuthenticationState.GetAuthenticationStateAsync()).User);

    private void ToggleLock()
    {
        if (!PrivateAccess.IsConfigured)
        {
            error = "Configure the Private area password before protecting notes.";
            return;
        }
        form.IsLocked = !form.IsLocked;
        dirty = true;
    }

    public async ValueTask DisposeAsync()
    {
        privacyCancellation.Cancel();
        if (privacyWatch is not null) await privacyWatch;
        privacyCancellation.Dispose();
    }
}
