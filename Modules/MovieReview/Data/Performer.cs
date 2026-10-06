namespace MovieReviews.Data;

public class Performer
{
	public int Id { get; set; }
	public string Name { get; set; } = "";

	public int? SkinColorId { get; set; }
	public SkinColor? SkinColor { get; set; }

	public int? BoobSizeId { get; set; }
	public BoobSize? BoobSize { get; set; }

	public int? AssSizeId { get; set; }
	public AssSize? AssSize { get; set; }

	public string? BoobsQuality { get; set; }
	public string? AssQuality { get; set; }
	public string? BjQuality { get; set; }
	public string? Moans { get; set; }
	public string? Notes { get; set; }

	public string? FaceQuality { get; set; }
	public string? BodySize { get; set; }
	public string? BodyQuality { get; set; }
	public string? BellyQuality { get; set; }
	public string? LegQuality { get; set; }
	public bool DoesNotSpeakEnglish { get; set; }
	public PerformerAgeCategory? AgeCategory { get; set; }

	public List<BjTag> BjTags { get; set; } = new();

	public bool IsWatchlisted { get; set; }
	public bool IsArchived { get; set; }

	public DateTime CreatedDate { get; set; }
	public DateTime LastUpdate { get; set; }

	public List<ScenePerformer> Scenes { get; set; } = new();
}
