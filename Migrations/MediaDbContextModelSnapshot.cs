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

        modelBuilder.Entity("MyNotes.Data.NoteGroup", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<string>("Category")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTime>("CreatedAtUtc")
                    .HasColumnType("datetime2");

                b.Property<string>("Description")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.HasKey("Id");

                b.HasIndex("Category");

                b.ToTable("NoteGroups");
            });

        modelBuilder.Entity("MyNotes.Data.NoteResearch", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<string>("Content")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("PersonalNoteId")
                    .HasColumnType("int");

                b.Property<int>("Position")
                    .HasColumnType("int");

                b.Property<int>("SourceKind")
                    .HasColumnType("int");

                b.Property<string>("SourceName")
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("SourceUrl")
                    .HasMaxLength(2048)
                    .HasColumnType("nvarchar(2048)");

                b.HasKey("Id");

                b.HasIndex("PersonalNoteId", "Position");

                b.ToTable("NoteResearch", t =>
                    {
                        t.HasCheckConstraint("CK_NoteResearch_SourceKind", "[SourceKind] IN (1, 2, 3)");
                    });
            });

        modelBuilder.Entity("MyNotes.Data.PersonalNote", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<string>("Category")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Conclusion")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTime>("CreatedAtUtc")
                    .HasColumnType("datetime2");

                b.Property<string>("Details")
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("Kind")
                    .HasColumnType("int");

                b.Property<DateOnly>("NoteDate")
                    .HasColumnType("date");

                b.Property<int?>("NoteGroupId")
                    .HasColumnType("int");

                b.Property<string>("Summary")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<DateTime>("UpdatedAtUtc")
                    .HasColumnType("datetime2");

                b.HasKey("Id");

                b.HasIndex("Category");

                b.HasIndex("NoteDate");

                b.HasIndex("NoteGroupId");

                b.ToTable("PersonalNotes", t =>
                    {
                        t.HasCheckConstraint("CK_PersonalNotes_Kind", "[Kind] IN (1, 2)");
                    });
            });

        modelBuilder.Entity("MyNotes.Data.NoteResearch", b =>
            {
                b.HasOne("MyNotes.Data.PersonalNote", "PersonalNote")
                    .WithMany("ResearchEntries")
                    .HasForeignKey("PersonalNoteId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("PersonalNote");
            });

        modelBuilder.Entity("MyNotes.Data.PersonalNote", b =>
            {
                b.HasOne("MyNotes.Data.NoteGroup", "NoteGroup")
                    .WithMany("Notes")
                    .HasForeignKey("NoteGroupId")
                    .OnDelete(DeleteBehavior.Restrict);

                b.Navigation("NoteGroup");
            });

        modelBuilder.Entity("MyNotes.Data.NoteGroup", b =>
            {
                b.Navigation("Notes");
            });

        modelBuilder.Entity("MyNotes.Data.PersonalNote", b =>
            {
                b.Navigation("ResearchEntries");
            });
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
