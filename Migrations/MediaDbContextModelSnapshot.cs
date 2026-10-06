using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MyNotes.Data;

#nullable disable

namespace MyNotes.Migrations;

[DbContext(typeof(MediaDbContext))]
public class MediaDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "9.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);
        modelBuilder.UseIdentityColumns();

        modelBuilder.Entity("MyNotes.Data.MediaItem", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            b.Property<int>("Kind").HasColumnType("int");
            b.Property<decimal?>("Rating").HasPrecision(4, 1).HasColumnType("decimal(4,1)");
            b.Property<int?>("ReleaseYear").HasColumnType("int");
            b.Property<string>("Review").HasColumnType("nvarchar(max)");
            b.Property<bool>("SavedToImdb").HasColumnType("bit");
            b.Property<bool>("SavedToLetterboxd").HasColumnType("bit");
            b.Property<string>("Title").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<DateTime>("UpdatedAtUtc").HasColumnType("datetime2");
            b.HasKey("Id");
            b.ToTable("MediaItems", t =>
            {
                t.HasCheckConstraint("CK_MediaItems_Kind", "[Kind] IN (1, 2)");
                t.HasCheckConstraint("CK_MediaItems_Rating", "[Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 10)");
                t.HasCheckConstraint("CK_MediaItems_ReleaseYear", "[ReleaseYear] IS NULL OR ([ReleaseYear] BETWEEN 1888 AND 2200)");
            });
        });

        modelBuilder.Entity("MyNotes.Data.Episode", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<int>("EpisodeNumber").HasColumnType("int");
            b.Property<int>("MediaItemId").HasColumnType("int");
            b.Property<decimal?>("Rating").HasPrecision(4, 1).HasColumnType("decimal(4,1)");
            b.Property<string>("Review").HasColumnType("nvarchar(max)");
            b.Property<int>("SeasonNumber").HasColumnType("int");
            b.Property<string>("Title").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.HasKey("Id");
            b.HasIndex("MediaItemId", "SeasonNumber", "EpisodeNumber").IsUnique();
            b.ToTable("Episodes", t =>
            {
                t.HasCheckConstraint("CK_Episodes_Numbers", "[SeasonNumber] >= 1 AND [EpisodeNumber] >= 0");
                t.HasCheckConstraint("CK_Episodes_Rating", "[Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 10)");
            });
        });

        modelBuilder.Entity("MyNotes.Data.Episode", b =>
        {
            b.HasOne("MyNotes.Data.MediaItem", "MediaItem")
                .WithMany("Episodes")
                .HasForeignKey("MediaItemId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("MediaItem");
        });

        modelBuilder.Entity("MyNotes.Data.MediaItem", b => b.Navigation("Episodes"));
    }
}
