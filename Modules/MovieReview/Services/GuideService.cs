using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MovieReviews.Services;

public class GuideService
{
    public const int MaxPictureBytes = 10 * 1024 * 1024;

    public async Task<List<GuidePictureInfo>> GetPicturesAsync(int sectionId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.GuidePictures.AsNoTracking().Where(p => p.GuideSectionId == sectionId)
            .OrderBy(p => p.Id).Select(p => new GuidePictureInfo(p.Id, p.FileName)).ToListAsync();
    }

    public async Task AddPictureAsync(int sectionId, string fileName, byte[] content)
    {
        if (content.Length == 0 || content.Length > MaxPictureBytes)
            throw new ArgumentException("Each picture must be between 1 byte and 10 MB.");
        var contentType = DetectPictureType(content);
        await using var db = await _factory.CreateDbContextAsync();
        if (!await db.GuideSections.AnyAsync(s => s.Id == sectionId))
            throw new InvalidOperationException("This vocabulary group no longer exists.");
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name)) name = "Picture";
        db.GuidePictures.Add(new GuidePicture
        {
            GuideSectionId = sectionId, FileName = name[..Math.Min(name.Length, 255)],
            ContentType = contentType, Content = content
        });
        await db.SaveChangesAsync();
    }

    public async Task DeletePictureAsync(int sectionId, int pictureId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.GuidePictures.Where(p => p.Id == pictureId && p.GuideSectionId == sectionId).ExecuteDeleteAsync();
    }

    private static string DetectPictureType(byte[] content)
    {
        var bytes = content.AsSpan();
        if (bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "image/png";
        if (bytes.StartsWith(new byte[] { 255, 216, 255 })) return "image/jpeg";
        if (bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8)) return "image/gif";
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8)) return "image/webp";
        throw new ArgumentException("Choose JPEG, PNG, GIF, or WebP pictures.");
    }

	private readonly IDbContextFactory<AppDbContext> _factory;

	public GuideService(IDbContextFactory<AppDbContext> factory)
	{
		_factory = factory;
	}

	public async Task<List<GuideSection>> GetAllAsync()
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.GuideSections
			.Include(s => s.Items.OrderBy(i => i.SortOrder))
			.OrderBy(s => s.SortOrder)
			.ToListAsync();
	}

	public async Task AddSectionAsync(string name, string icon)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var max = await db.GuideSections.AnyAsync() ? await db.GuideSections.MaxAsync(s => s.SortOrder) : 0;
		db.GuideSections.Add(new GuideSection { Name = name, Icon = icon, SortOrder = max + 1 });
		await db.SaveChangesAsync();
	}

	public async Task UpdateSectionAsync(GuideSection section)
	{
		await using var db = await _factory.CreateDbContextAsync();
		db.GuideSections.Update(section);
		await db.SaveChangesAsync();
	}

	public async Task DeleteSectionAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var items = await db.GuideItems.Where(i => i.GuideSectionId == id).ToListAsync();
		db.GuideItems.RemoveRange(items);
		var s = await db.GuideSections.FindAsync(id);
		if (s is not null) db.GuideSections.Remove(s);
		await db.SaveChangesAsync();
	}

	public async Task AddItemAsync(int sectionId, string text, string? description)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var max = await db.GuideItems.AnyAsync(i => i.GuideSectionId == sectionId)
			? await db.GuideItems.Where(i => i.GuideSectionId == sectionId).MaxAsync(i => i.SortOrder)
			: 0;
		db.GuideItems.Add(new GuideItem
		{
			GuideSectionId = sectionId,
			Text = text,
			Description = description,
			SortOrder = max + 1
		});
		await db.SaveChangesAsync();
	}

	public async Task UpdateItemAsync(GuideItem item)
	{
		await using var db = await _factory.CreateDbContextAsync();
		db.GuideItems.Update(item);
		await db.SaveChangesAsync();
	}

	public async Task DeleteItemAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var i = await db.GuideItems.FindAsync(id);
		if (i is null) return;
		db.GuideItems.Remove(i);
		await db.SaveChangesAsync();
	}

	public async Task MoveItemAsync(int id, bool up)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var item = await db.GuideItems.FindAsync(id);
		if (item is null) return;

		var siblings = await db.GuideItems
			.Where(i => i.GuideSectionId == item.GuideSectionId)
			.OrderBy(i => i.SortOrder)
			.ToListAsync();

		var index = siblings.FindIndex(i => i.Id == id);
		var swapWith = up ? index - 1 : index + 1;
		if (swapWith < 0 || swapWith >= siblings.Count) return;

		(siblings[index].SortOrder, siblings[swapWith].SortOrder) =
			(siblings[swapWith].SortOrder, siblings[index].SortOrder);
		await db.SaveChangesAsync();
	}
}
