namespace MovieReviews.Data;

public class SkinColor
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public bool IsDeleted { get; set; }
	public List<Performer> Performers { get; set; } = new();
}

public class BoobSize
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public bool IsDeleted { get; set; }
	public List<Performer> Performers { get; set; } = new();
}

public class AssSize
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public bool IsDeleted { get; set; }
	public List<Performer> Performers { get; set; } = new();
}

public class BjTag
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public bool IsDeleted { get; set; }
	public List<Performer> Performers { get; set; } = new();
}