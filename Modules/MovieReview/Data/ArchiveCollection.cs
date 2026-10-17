namespace MovieReviews.Data;

public class ArchiveCollection
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ArchiveName { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public byte[] OriginalArchive { get; set; } = [];
    public DateTime ImportedUtc { get; set; }
    public List<ArchiveItem> Items { get; set; } = [];
}

public class ArchiveItem
{
    public int Id { get; set; }
    public int ArchiveCollectionId { get; set; }
    public ArchiveCollection Collection { get; set; } = null!;
    public int SourceRow { get; set; }
    public string Title { get; set; } = "";
    public string? Url { get; set; }
    public string Kind { get; set; } = "Bookmark";
    public string SourceNote { get; set; } = "";
    public string SourceData { get; set; } = "";
    public string? AttachmentName { get; set; }
    public string Notes { get; set; } = "";
    public string Status { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Progress { get; set; } = "";
    public int? SceneId { get; set; }
    public Scene? Scene { get; set; }
    public int? PerformerId { get; set; }
    public Performer? Performer { get; set; }
}
