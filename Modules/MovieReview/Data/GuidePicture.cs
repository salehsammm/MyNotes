namespace MovieReviews.Data;

public class GuidePicture
{
    public int Id { get; set; }
    public int GuideSectionId { get; set; }
    public GuideSection Section { get; set; } = null!;
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public byte[] Content { get; set; } = [];
}

public record GuidePictureInfo(int Id, string FileName);
