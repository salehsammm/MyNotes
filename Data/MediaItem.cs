using System.ComponentModel.DataAnnotations;

namespace MyNotes.Data;

public enum MediaKind
{
    Movie = 1,
    Series = 2
}

public class MediaItem
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public MediaKind Kind { get; set; }

    [Range(1888, 2200)]
    public int? ReleaseYear { get; set; }

    [Range(0, 10)]
    public decimal? Rating { get; set; }

    public string? Review { get; set; }

    public bool SavedToImdb { get; set; }
    public bool SavedToLetterboxd { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Episode> Episodes { get; set; } = [];
}
