namespace MovieReviews.Data;

public class Scene
{
	public int Id { get; set; }
	public string Title { get; set; } = "";
	public string? Studio { get; set; }
	public bool IsDraft { get; set; }
	public bool IsPinnedDraft { get; set; }
	public int? ResumeAtSeconds { get; set; }

	// overall
	public Rating? OverallRating { get; set; }
	public int OverallModifier { get; set; }   // -2..+2
	public string? Verdict { get; set; }

	// setup
	public Rating? SetupRating { get; set; }
	public int SetupModifier { get; set; }
	public string? SetupNotes { get; set; }
	public bool SetupSpeaksNoEnglish { get; set; }

	// sex
	public Rating? SexRating { get; set; }
	public int SexModifier { get; set; }
	public string? SexNotes { get; set; }

	// tags & flags
	public bool IsPov { get; set; }
	public string? Tags { get; set; }

	public List<ScenePerformer> Performers { get; set; } = new();
	public List<Position> Positions { get; set; } = new();
	public List<Location> Locations { get; set; } = new();

	public DateTime CreatedDate { get; set; }
	public DateTime LastUpdate { get; set; }
}
