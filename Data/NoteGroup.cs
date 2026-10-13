using System.ComponentModel.DataAnnotations;

namespace MyNotes.Data;

public class NoteGroup
{
    public int Id { get; set; }
    [Required, StringLength(200)]
    public string Name { get; set; } = "";
    [StringLength(100)]
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<PersonalNote> Notes { get; set; } = [];
}

public enum ResearchSourceKind { Website = 1, AI = 2, Other = 3 }

public class NoteResearch
{
    public int Id { get; set; }
    public int PersonalNoteId { get; set; }
    public PersonalNote PersonalNote { get; set; } = null!;
    public ResearchSourceKind SourceKind { get; set; } = ResearchSourceKind.Website;
    [StringLength(200)]
    public string? SourceName { get; set; }
    [StringLength(2048)]
    public string? SourceUrl { get; set; }
    [Required]
    public string Content { get; set; } = "";
    public int Position { get; set; }
}
