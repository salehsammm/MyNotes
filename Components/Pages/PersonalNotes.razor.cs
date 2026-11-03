using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MyNotes.Data;

namespace MyNotes.Components.Pages;

public partial class PersonalNotes
{
    private List<PersonalNote> notes = [];
    private List<string> categories = [];
    private PersonalNote form = new();
    private EditContext editContext = default!;
    private int? selectedId;
    private int resultCount;
    private int resultLimit = 50;
    private bool busy;
    private bool dirty;
    private bool confirmDelete;
    private string? error;
    private string? message;
    private Func<Task>? pendingAction;
    private string searchText = "";
    private string matchMode = "all";
    private string searchScope = "all";
    private string kindFilter = "All";
    private bool favoritesOnly;
    private string categoryFilter = "";
    private DateOnly? fromDate;
    private DateOnly? toDate;
    private string sortOrder = "date-desc";
    private static readonly TimeZoneInfo NoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
    private static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NoteTimeZone));
    private static string LocalTimestamp(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), NoteTimeZone).ToString("yyyy-MM-dd HH:mm");

    protected override async Task OnInitializedAsync()
    {
        SetForm(new() { NoteDate = Today });
        await Run(RefreshResults);
    }

    private void SetForm(PersonalNote note)
    {
        form = note;
        editContext = new(form);
        editContext.OnFieldChanged += (_, _) => dirty = true;
        dirty = false;
        confirmDelete = false;
        pendingAction = null;
        showGroupEditor = groupDirty = false;
    }

    private void MarkDirty(ChangeEventArgs _) => dirty = true;

    private Task RequestNew(PersonalNoteKind kind) => RequestChange(() =>
    {
        selectedId = null;
        error = message = null;
        SetForm(new() { Kind = kind, NoteDate = Today, Title = kind == PersonalNoteKind.Diary ? $"Diary · {Today:yyyy-MM-dd}" : "", NoteGroupId = kind == PersonalNoteKind.Note && groupFilter > 0 ? groupFilter : null, Category = kind == PersonalNoteKind.Note && groupFilter > 0 ? groups.FirstOrDefault(g => g.Id == groupFilter)?.Category : null });
        return Task.CompletedTask;
    });

    private Task RequestSelect(int id) => RequestChange(() => Run(async () =>
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        var note = await db.PersonalNotes.AsNoTracking().Include(x => x.NoteGroup).Include(x => x.ResearchEntries).SingleOrDefaultAsync(x => x.Id == id);
        if (note is null) { error = "This note no longer exists. Search again to refresh the list."; return; }
        selectedId = note.Id;
        note.Category = EffectiveCategory(note);
        note.ResearchEntries = note.ResearchEntries.OrderBy(e => e.Position).ThenBy(e => e.Id).ToList();
        SetForm(note);
    }));

    private Task RequestChange(Func<Task> action)
    {
        if (busy) return Task.CompletedTask;
        if (dirty || groupDirty) { pendingAction = action; pendingGroupOnly = false; return Task.CompletedTask; }
        return action();
    }

    private async Task DiscardChanges()
    {
        var action = pendingAction;
        pendingAction = null;
        if (!pendingGroupOnly) dirty = false;
        groupDirty = false;
        pendingGroupOnly = false;
        if (action is not null) await action();
    }

    private async Task BeforeNavigation(LocationChangingContext context)
    {
        if ((dirty || groupDirty) && !await JS.InvokeAsync<bool>("confirm", "Discard unsaved changes and leave this page?")) context.PreventNavigation();
    }

    private async Task Run(Func<Task> action)
    {
        if (busy) return;
        busy = true;
        error = message = null;
        try { await action(); }
        catch (Exception ex) { Logger.LogError(ex, "Personal notes operation failed"); error = "Could not update your notes. Your writing is still here; please try again."; }
        finally { busy = false; }
    }

    private Task Search() { resultLimit = 50; return Run(RefreshResults); }
    private Task FilterKind(string kind) { kindFilter = kind; return Search(); }
    private Task FilterFavorites() { favoritesOnly = !favoritesOnly; return Search(); }
    private void ToggleFavorite() { form.IsFavorite = !form.IsFavorite; dirty = true; }
    private Task LoadMore() { resultLimit += 50; return Run(RefreshResults); }
    private Task ClearSearch()
    {
        searchText = categoryFilter = "";
        groupFilter = 0;
        kindFilter = "All";
        favoritesOnly = false;
        matchMode = searchScope = "all";
        fromDate = toDate = null;
        sortOrder = "date-desc";
        return Search();
    }

    private async Task RefreshResults()
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            error = "From date must be on or before To date.";
            return;
        }
        await using var db = await DbFactory.CreateDbContextAsync();
        var query = db.PersonalNotes.AsNoTracking().Include(n => n.NoteGroup).AsQueryable();
        if (groupFilter > 0) query = query.Where(n => n.NoteGroupId == groupFilter);
        if (groupFilter == -1) query = query.Where(n => n.NoteGroupId == null);
        if (kindFilter == "Notes") query = query.Where(n => n.Kind == PersonalNoteKind.Note);
        if (kindFilter == "Diary") query = query.Where(n => n.Kind == PersonalNoteKind.Diary);
        if (favoritesOnly) query = query.Where(n => n.IsFavorite);
        if (categoryFilter == "__none") query = query.Where(n => n.NoteGroupId != null ? n.NoteGroup!.Category == null : n.Category == null);
        else if (!string.IsNullOrEmpty(categoryFilter)) query = query.Where(n => (n.NoteGroupId != null ? n.NoteGroup!.Category : n.Category) == categoryFilter);
        if (fromDate is DateOnly first) query = query.Where(n => n.NoteDate >= first);
        if (toDate is DateOnly last) query = query.Where(n => n.NoteDate <= last);
        var terms = matchMode == "phrase" ? new[] { searchText.Trim() } : searchText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        terms = terms.Where(t => t.Length > 0).ToArray();
        query = ApplyTextSearch(query, terms);
        resultCount = await query.CountAsync();
        query = sortOrder switch
        {
            "date-asc" => query.OrderBy(n => n.NoteDate).ThenBy(n => n.Id),
            "updated" => query.OrderByDescending(n => n.UpdatedAtUtc).ThenByDescending(n => n.Id),
            "title" => query.OrderBy(n => n.Title).ThenBy(n => n.Id),
            _ => query.OrderByDescending(n => n.NoteDate).ThenByDescending(n => n.UpdatedAtUtc).ThenByDescending(n => n.Id)
        };
        notes = await query.Take(resultLimit).ToListAsync();
        groups = await db.NoteGroups.AsNoTracking().OrderBy(g => g.Name).ToListAsync();
        groupCounts = await db.PersonalNotes.Where(n => n.NoteGroupId != null).GroupBy(n => n.NoteGroupId!.Value).Select(g => new { Id = g.Key, Count = g.Count() }).ToDictionaryAsync(g => g.Id, g => g.Count);
        categories = await db.PersonalNotes.Where(n => n.NoteGroupId == null && n.Category != null).Select(n => n.Category!)
            .Union(db.NoteGroups.Where(g => g.Category != null).Select(g => g.Category!)).OrderBy(c => c).ToListAsync();
    }

    private Task SaveNote() => Run(async () =>
    {
        if (string.IsNullOrWhiteSpace(form.Title) || !Enum.IsDefined(form.Kind) || form.NoteDate == default)
        {
            error = "Enter a title and a valid note date.";
            return;
        }
        await using var db = await DbFactory.CreateDbContextAsync();
        if (ValidateResearch() is { } researchError) { error = researchError; return; }
        NoteGroup? group = null;
        if (form.NoteGroupId is int groupId)
        {
            group = await db.NoteGroups.FindAsync(groupId);
            if (group is null) { error = "This group no longer exists. Choose another group or No group."; return; }
        }
        var note = selectedId is int id ? await db.PersonalNotes.Include(n => n.ResearchEntries).SingleOrDefaultAsync(n => n.Id == id) : new PersonalNote();
        if (note is null) { error = "This note no longer exists. Your writing is still here."; return; }
        note.Title = form.Title.Trim();
        note.Kind = form.Kind;
        note.IsFavorite = form.IsFavorite;
        note.NoteDate = form.NoteDate;
        var category = string.IsNullOrWhiteSpace(form.Category) ? null : form.Category.Trim();
        note.NoteGroupId = group?.Id;
        note.Category = group is not null ? group.Category : category is null ? null : await db.PersonalNotes.Where(n => n.Category == category).Select(n => n.Category).FirstOrDefaultAsync() ?? category;
        // Preserve the user's paragraphs and Unicode text exactly as written.
        note.Details = form.Details;
        note.Conclusion = form.Conclusion;
        note.Summary = form.Summary;
        note.UpdatedAtUtc = DateTime.UtcNow;
        UpdateResearch(db, note);
        if (selectedId is null) db.PersonalNotes.Add(note);
        await db.SaveChangesAsync();
        selectedId = note.Id;
        form.Category = note.Category;
        form.CreatedAtUtc = note.CreatedAtUtc;
        form.UpdatedAtUtc = note.UpdatedAtUtc;
        form.ResearchEntries = note.ResearchEntries.Where(e => db.Entry(e).State != EntityState.Detached && db.Entry(e).State != EntityState.Deleted).OrderBy(e => e.Position).Select(e => new NoteResearch { Id = e.Id, SourceKind = e.SourceKind, SourceName = e.SourceName, SourceUrl = e.SourceUrl, Content = e.Content, Position = e.Position }).ToList();
        dirty = false;
        pendingAction = null;
        await RefreshResults();
        message = "Note saved.";
    });

    private Task DeleteNote() => Run(async () =>
    {
        if (selectedId is not int id) return;
        await using var db = await DbFactory.CreateDbContextAsync();
        var note = await db.PersonalNotes.FindAsync(id);
        if (note is not null) { db.PersonalNotes.Remove(note); await db.SaveChangesAsync(); }
        selectedId = null;
        SetForm(new() { NoteDate = Today });
        await RefreshResults();
        message = "Note deleted.";
    });

    private static string Excerpt(PersonalNote note)
    {
        var text = note.Summary ?? note.Details ?? note.Conclusion ?? "";
        text = text.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return text.Length > 100 ? text[..100] + "…" : text;
    }
}
