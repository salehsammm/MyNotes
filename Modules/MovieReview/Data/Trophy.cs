namespace MovieReviews.Data;

public class TrophyCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public List<TrophyEntry> Entries { get; set; } = new();
}

public class TrophyEntry
{
    public int Id { get; set; }
    public int TrophyCategoryId { get; set; }
    public TrophyCategory Category { get; set; } = null!;
    public int Rank { get; set; }
    public string Description { get; set; } = "";
    public string? References { get; set; }
}
