namespace MovieReviews.Data;

public class ScenePerformer
{
	public int Id { get; set; }

	public int SceneId { get; set; }
	public Scene Scene { get; set; } = null!;

	public int PerformerId { get; set; }
	public Performer Performer { get; set; } = null!;

	// per-scene overrides (leave null to use the performer's defaults)
	public string? SkinOverride { get; set; }
	public string? BoobsOverride { get; set; }
	public string? AssOverride { get; set; }
	public string? BjOverride { get; set; }
	public string? MoansOverride { get; set; }

	public string? Notes { get; set; }
}