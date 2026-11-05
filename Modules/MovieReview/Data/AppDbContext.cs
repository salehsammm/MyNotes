using Microsoft.EntityFrameworkCore;

namespace MovieReviews.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistScene> PlaylistScenes => Set<PlaylistScene>();
	public DbSet<Scene> Scenes => Set<Scene>();
    public DbSet<ArchiveCollection> ArchiveCollections => Set<ArchiveCollection>();
    public DbSet<ArchiveItem> ArchiveItems => Set<ArchiveItem>();
	public DbSet<Studio> Studios => Set<Studio>();
	public DbSet<Performer> Performers => Set<Performer>();
	public DbSet<ScenePerformer> ScenePerformers => Set<ScenePerformer>();
	public DbSet<SkinColor> SkinColors => Set<SkinColor>();
	public DbSet<BoobSize> BoobSizes => Set<BoobSize>();
	public DbSet<AssSize> AssSizes => Set<AssSize>();
	public DbSet<BjTag> BjTags => Set<BjTag>();
	public DbSet<Idea> Ideas => Set<Idea>();
	public DbSet<Note> Notes => Set<Note>();
	public DbSet<GuideSection> GuideSections => Set<GuideSection>();
	public DbSet<GuideItem> GuideItems => Set<GuideItem>();
    public DbSet<GuidePicture> GuidePictures => Set<GuidePicture>();
	public DbSet<ResearchDoc> ResearchDocs => Set<ResearchDoc>();
	public DbSet<ResearchSection> ResearchSections => Set<ResearchSection>();
	public DbSet<TrophyCategory> TrophyCategories => Set<TrophyCategory>();
	public DbSet<TrophyEntry> TrophyEntries => Set<TrophyEntry>();

	protected override void OnModelCreating(ModelBuilder b)
	{
        b.Entity<Playlist>().Property(p => p.Name).HasMaxLength(160);
        b.Entity<Playlist>().Property(p => p.Description).HasMaxLength(10000);
        b.Entity<Playlist>().HasIndex(p => p.Name).IsUnique();
        b.Entity<PlaylistScene>().Property(p => p.Note).HasMaxLength(10000);
        b.Entity<PlaylistScene>().ToTable(t => t.HasCheckConstraint("CK_PlaylistScene_Position", "[Position] > 0"));
        b.Entity<PlaylistScene>().HasKey(p => new { p.PlaylistId, p.SceneId });
        b.Entity<PlaylistScene>().HasOne(p => p.Playlist).WithMany(p => p.Scenes)
            .HasForeignKey(p => p.PlaylistId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<PlaylistScene>().HasOne(p => p.Scene).WithMany()
            .HasForeignKey(p => p.SceneId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<GuidePicture>().Property(p => p.FileName).HasMaxLength(255);
        b.Entity<GuidePicture>().Property(p => p.ContentType).HasMaxLength(32);
        b.Entity<GuidePicture>().HasOne(p => p.Section).WithMany()
            .HasForeignKey(p => p.GuideSectionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ArchiveCollection>().Property(x => x.Sha256).HasMaxLength(64);
        b.Entity<ArchiveCollection>().HasIndex(x => x.Sha256).IsUnique();
        b.Entity<ArchiveItem>().HasIndex(x => new { x.ArchiveCollectionId, x.SourceRow }).IsUnique();
        b.Entity<ArchiveItem>().HasOne(x => x.Collection).WithMany(x => x.Items)
            .HasForeignKey(x => x.ArchiveCollectionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ArchiveItem>().HasOne(x => x.Scene).WithMany()
            .HasForeignKey(x => x.SceneId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<ArchiveItem>().HasOne(x => x.Performer).WithMany()
            .HasForeignKey(x => x.PerformerId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Performer>().ToTable(t => t.HasCheckConstraint("CK_Performer_RankingPosition", "[RankingPosition] IS NULL OR [RankingPosition] > 0"));
		b.Entity<SkinColor>().ToTable("SkinColor");
		b.Entity<BoobSize>().ToTable("BoobSize");
		b.Entity<AssSize>().ToTable("AssSize");
		b.Entity<BjTag>().ToTable("BjTag");
		b.Entity<Studio>().Property(s => s.Name).HasMaxLength(450);

		b.Entity<ScenePerformer>()
			.HasOne(sp => sp.Scene)
			.WithMany(s => s.Performers)
			.HasForeignKey(sp => sp.SceneId);

		b.Entity<ScenePerformer>()
			.HasOne(sp => sp.Performer)
			.WithMany(p => p.Scenes)
			.HasForeignKey(sp => sp.PerformerId);

		b.Entity<Performer>()
			.HasMany(p => p.BjTags)
			.WithMany(t => t.Performers)
			.UsingEntity(j => j.ToTable("PerformerBjTags"));

		b.Entity<Scene>()
			.Property(s => s.Positions)
			.HasConversion(
				v => string.Join(',', v),
				v => v.Length == 0
					? new List<Position>()
					: v.Split(',', StringSplitOptions.RemoveEmptyEntries)
					   .Select(Enum.Parse<Position>).ToList())
			.Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<Position>>(
				(a, c) => a!.SequenceEqual(c!),
				a => a.Aggregate(0, (h, e) => HashCode.Combine(h, e.GetHashCode())),
				a => a.ToList()));

		b.Entity<Scene>()
			.Property(s => s.Locations)
			.HasConversion(
				v => string.Join(',', v),
				v => v.Length == 0
					? new List<Location>()
					: v.Split(',', StringSplitOptions.RemoveEmptyEntries)
					   .Select(Enum.Parse<Location>).ToList())
			.Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<Location>>(
				(a, c) => a!.SequenceEqual(c!),
				a => a.Aggregate(0, (h, e) => HashCode.Combine(h, e.GetHashCode())),
				a => a.ToList()));

		b.Entity<ResearchSection>()
			.HasOne(s => s.Parent)
			.WithMany(s => s.Children)
			.HasForeignKey(s => s.ParentId)
			.OnDelete(DeleteBehavior.Restrict);

		b.Entity<ResearchSection>()
			.HasOne(s => s.Doc)
			.WithMany(d => d.Sections)
			.HasForeignKey(s => s.ResearchDocId)
			.OnDelete(DeleteBehavior.Cascade);

		b.Entity<TrophyCategory>()
			.HasMany(c => c.Entries)
			.WithOne(e => e.Category)
			.HasForeignKey(e => e.TrophyCategoryId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<TrophyCategory>().Property(c => c.Name).HasMaxLength(160);
	}
}
