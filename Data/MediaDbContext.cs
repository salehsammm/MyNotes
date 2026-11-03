using Microsoft.EntityFrameworkCore;

namespace MyNotes.Data;

public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<PersonalNote> PersonalNotes => Set<PersonalNote>();
    public DbSet<NoteGroup> NoteGroups => Set<NoteGroup>();
    public DbSet<NoteResearch> NoteResearch => Set<NoteResearch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NoteGroup>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.Property(x => x.Description).HasColumnType("nvarchar(max)");
            entity.HasIndex(x => x.Category);
        });
        modelBuilder.Entity<NoteResearch>(entity =>
        {
            entity.Property(x => x.SourceName).HasMaxLength(200);
            entity.Property(x => x.SourceUrl).HasMaxLength(2048);
            entity.Property(x => x.Content).HasColumnType("nvarchar(max)").IsRequired();
            entity.HasOne(x => x.PersonalNote).WithMany(x => x.ResearchEntries)
                .HasForeignKey(x => x.PersonalNoteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.PersonalNoteId, x.Position });
            entity.ToTable(t => t.HasCheckConstraint("CK_NoteResearch_SourceKind", "[SourceKind] IN (1, 2, 3)"));
        });
        modelBuilder.Entity<PersonalNote>(entity =>
        {
            entity.Property(x => x.IsLocked).IsConcurrencyToken();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.Property(x => x.Details).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Conclusion).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Summary).HasColumnType("nvarchar(max)");
            entity.HasOne(x => x.NoteGroup).WithMany(x => x.Notes)
                .HasForeignKey(x => x.NoteGroupId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.NoteDate);
            entity.HasIndex(x => x.Category);
            entity.ToTable(t => t.HasCheckConstraint("CK_PersonalNotes_Kind", "[Kind] IN (1, 2)"));
        });

        modelBuilder.Entity<MediaItem>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Rating).HasPrecision(4, 1);
            entity.Property(x => x.Review).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Kind).HasConversion<int>();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MediaItems_Rating", "[Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 10)");
                t.HasCheckConstraint("CK_MediaItems_Kind", "[Kind] IN (1, 2)");
                t.HasCheckConstraint("CK_MediaItems_ReleaseYear", "[ReleaseYear] IS NULL OR ([ReleaseYear] BETWEEN 1888 AND 2200)");
            });
        });

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.Rating).HasPrecision(4, 1);
            entity.Property(x => x.Review).HasColumnType("nvarchar(max)");
            entity.HasIndex(x => new { x.MediaItemId, x.SeasonNumber, x.EpisodeNumber }).IsUnique();
            entity.HasOne(x => x.MediaItem).WithMany(x => x.Episodes).HasForeignKey(x => x.MediaItemId).OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Episodes_Numbers", "[SeasonNumber] >= 1 AND [EpisodeNumber] >= 0");
                t.HasCheckConstraint("CK_Episodes_Rating", "[Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 10)");
            });
        });
    }
}
