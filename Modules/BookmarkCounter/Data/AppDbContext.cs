using Microsoft.EntityFrameworkCore;

namespace BookmarkCounter.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<BookmarkCount> BookmarkCounts => Set<BookmarkCount>();
	public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
	public DbSet<ChecklistProgress> ChecklistProgress => Set<ChecklistProgress>();
	public DbSet<Question> Questions => Set<Question>();

	protected override void OnModelCreating(ModelBuilder b)
	{
		var bc = b.Entity<BookmarkCount>();
		bc.Property(x => x.FolderName).HasMaxLength(100).IsRequired();
		bc.HasIndex(x => new { x.FolderName, x.RecordedAt });

		var ci = b.Entity<ChecklistItem>();
		ci.Property(x => x.Title).HasMaxLength(200).IsRequired();
		ci.Property(x => x.CarryOverIfMissed).HasDefaultValue(true);
		ci.Property(x => x.Icon).HasMaxLength(20);
		ci.Property(x => x.BackgroundColor).HasMaxLength(7);

		var cp = b.Entity<ChecklistProgress>();
		cp.HasOne(x => x.Item).WithMany(x => x.Progress)
		  .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Cascade);
		cp.HasIndex(x => new { x.ItemId, x.PeriodStart }).IsUnique();

		b.Entity<Question>().Property(x => x.Text).HasMaxLength(500).IsRequired();
		b.Entity<Question>().Property(x => x.Answer).HasMaxLength(4000);
	}
}
