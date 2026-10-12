using System.ComponentModel.DataAnnotations;

namespace MyNotes.Data;

public enum PersonalNoteKind { Note = 1, Diary = 2 }

public class PersonalNote
{
    public int Id { get; set; }
    [Required, StringLength(200)]
    public string Title { get; set; } = "";
    public PersonalNoteKind Kind { get; set; } = PersonalNoteKind.Note;
    public DateOnly NoteDate { get; set; }
    [StringLength(100)]
    public string? Category { get; set; }
    public string? Details { get; set; }
    public string? Conclusion { get; set; }
    public string? Summary { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
