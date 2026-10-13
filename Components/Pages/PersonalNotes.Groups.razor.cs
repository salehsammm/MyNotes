using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MyNotes.Data;

namespace MyNotes.Components.Pages;

public partial class PersonalNotes
{
    private List<NoteGroup> groups = [];
    private Dictionary<int, int> groupCounts = [];
    private int groupFilter;
    private NoteGroup groupForm = new();
    private bool showGroupEditor;
    private bool groupDirty;
    private bool confirmGroupDelete;
    private bool pendingGroupOnly;
    private int GroupCount(int id) => groupCounts.GetValueOrDefault(id);
    private static string? EffectiveCategory(PersonalNote note) => note.NoteGroup is not null ? note.NoteGroup.Category : note.Category;

    private void MarkGroupDirty(ChangeEventArgs _) => groupDirty = true;

    private Task RequestGroupChange(Func<Task> action)
    {
        if (busy) return Task.CompletedTask;
        if (groupDirty) { pendingAction = action; pendingGroupOnly = true; return Task.CompletedTask; }
        return action();
    }

    private Task EditGroup(int? id) => RequestGroupChange(() =>
    {
        var group = groups.FirstOrDefault(g => g.Id == id);
        groupForm = group is null ? new() : new() { Id = group.Id, Name = group.Name, Category = group.Category, Description = group.Description };
        showGroupEditor = true;
        groupDirty = false;
        confirmGroupDelete = false;
        return Task.CompletedTask;
    });

    private Task CancelGroup() => RequestGroupChange(() =>
    {
        showGroupEditor = groupDirty = false;
        return Task.CompletedTask;
    });

    private Task OpenGroup(int id) => RequestChange(() => Run(async () =>
    {
        groupFilter = id;
        searchText = categoryFilter = "";
        kindFilter = "All";
        fromDate = toDate = null;
        resultLimit = 50;
        selectedId = null;
        SetForm(new() { NoteDate = Today, NoteGroupId = id, Category = groups.FirstOrDefault(g => g.Id == id)?.Category });
        await RefreshResults();
    }));

    private Task RequestTopic(int groupId) => RequestChange(() =>
    {
        selectedId = null;
        SetForm(new() { NoteDate = Today, NoteGroupId = groupId, Category = groups.FirstOrDefault(g => g.Id == groupId)?.Category });
        return Task.CompletedTask;
    });

    private void GroupChanged()
    {
        if (form.NoteGroupId is int id) form.Category = groups.FirstOrDefault(g => g.Id == id)?.Category;
        dirty = true;
    }

    private Task SaveGroup() => Run(async () =>
    {
        if (string.IsNullOrWhiteSpace(groupForm.Name)) { error = "Enter a group name."; return; }
        await using var db = await DbFactory.CreateDbContextAsync();
        var creating = groupForm.Id == 0;
        var group = creating ? new NoteGroup() : await db.NoteGroups.FindAsync(groupForm.Id);
        if (group is null) { error = "This group no longer exists."; return; }
        group.Name = groupForm.Name.Trim();
        group.Category = string.IsNullOrWhiteSpace(groupForm.Category) ? null : groupForm.Category.Trim();
        group.Description = groupForm.Description;
        if (creating) db.NoteGroups.Add(group);
        await db.SaveChangesAsync();
        showGroupEditor = groupDirty = false;
        confirmGroupDelete = false;
        if (creating) groupFilter = group.Id;
        await RefreshResults();
        if (creating && !dirty)
        {
            selectedId = null;
            SetForm(new() { NoteDate = Today, NoteGroupId = group.Id, Category = group.Category });
        }
        else if (form.NoteGroupId == group.Id) form.Category = group.Category;
        message = "Group saved. Add a separate topic for each point.";
    });

    private Task DeleteGroup() => Run(async () =>
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        if (await db.PersonalNotes.AnyAsync(n => n.NoteGroupId == groupForm.Id))
        {
            error = "This group has topics. Move them to another group or choose No group before deleting it.";
            return;
        }
        var group = await db.NoteGroups.FindAsync(groupForm.Id);
        if (group is not null) { db.NoteGroups.Remove(group); await db.SaveChangesAsync(); }
        if (groupFilter == groupForm.Id) groupFilter = 0;
        if (form.NoteGroupId == groupForm.Id) form.NoteGroupId = null;
        showGroupEditor = groupDirty = false;
        await RefreshResults();
        message = "Empty group deleted.";
    });

    private void AddResearch()
    {
        form.ResearchEntries.Add(new() { Position = form.ResearchEntries.Count + 1 });
        dirty = true;
    }

    private void RemoveResearch(NoteResearch entry)
    {
        form.ResearchEntries.Remove(entry);
        dirty = true;
    }

    private static string? SafeSourceUrl(string? value) => Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https") ? uri.AbsoluteUri : null;

    private string? ValidateResearch()
    {
        for (var index = 0; index < form.ResearchEntries.Count; index++)
        {
            var entry = form.ResearchEntries[index];
            var validation = new List<ValidationResult>();
            if (!Validator.TryValidateObject(entry, new(entry), validation, true)) return $"Research {index + 1}: {validation[0].ErrorMessage}";
            if (!Enum.IsDefined(entry.SourceKind)) return $"Research {index + 1}: choose a source type.";
            if (!string.IsNullOrWhiteSpace(entry.SourceUrl) && SafeSourceUrl(entry.SourceUrl) is null) return $"Research {index + 1}: use a full http:// or https:// source link.";
        }
        return null;
    }

    private void UpdateResearch(MediaDbContext db, PersonalNote note)
    {
        var keep = form.ResearchEntries.Where(e => e.Id > 0).Select(e => e.Id).ToHashSet();
        foreach (var removed in note.ResearchEntries.Where(e => !keep.Contains(e.Id)).ToList()) db.NoteResearch.Remove(removed);
        for (var index = 0; index < form.ResearchEntries.Count; index++)
        {
            var input = form.ResearchEntries[index];
            var entry = input.Id == 0 ? new NoteResearch() : note.ResearchEntries.Single(e => e.Id == input.Id);
            entry.SourceKind = input.SourceKind;
            entry.SourceName = string.IsNullOrWhiteSpace(input.SourceName) ? null : input.SourceName.Trim();
            entry.SourceUrl = string.IsNullOrWhiteSpace(input.SourceUrl) ? null : input.SourceUrl.Trim();
            entry.Content = input.Content;
            entry.Position = index + 1;
            if (input.Id == 0) note.ResearchEntries.Add(entry);
        }
    }
}
