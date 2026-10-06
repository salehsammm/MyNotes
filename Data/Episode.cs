using System.ComponentModel.DataAnnotations;

namespace MyNotes.Data;

public class Episode
{
    public int Id { get; set; }
    public int MediaItemId { get; set; }
    public MediaItem MediaItem { get; set; } = null!;

    [Range(1, 999)]
    public int SeasonNumber { get; set; } = 1;

    [Range(0, 9999)]
    public int EpisodeNumber { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    [Range(0, 10)]
    public decimal? Rating { get; set; }

    public string? Review { get; set; }
}
