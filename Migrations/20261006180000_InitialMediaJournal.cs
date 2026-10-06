using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using MyNotes.Data;

#nullable disable

namespace MyNotes.Migrations;

[DbContext(typeof(MediaDbContext))]
[Migration("20261006180000_InitialMediaJournal")]
public class InitialMediaJournal : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MediaItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Kind = table.Column<int>(type: "int", nullable: false),
                ReleaseYear = table.Column<int>(type: "int", nullable: true),
                Rating = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                Review = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MediaItems", x => x.Id);
                table.CheckConstraint("CK_MediaItems_Kind", "[Kind] IN (1, 2)");
                table.CheckConstraint("CK_MediaItems_Rating", "[Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 10)");
                table.CheckConstraint("CK_MediaItems_ReleaseYear", "[ReleaseYear] IS NULL OR ([ReleaseYear] BETWEEN 1888 AND 2200)");
            });

        migrationBuilder.CreateTable(
            name: "Episodes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                MediaItemId = table.Column<int>(type: "int", nullable: false),
                SeasonNumber = table.Column<int>(type: "int", nullable: false),
                EpisodeNumber = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Rating = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                Review = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Episodes", x => x.Id);
                table.CheckConstraint("CK_Episodes_Numbers", "[SeasonNumber] >= 1 AND [EpisodeNumber] >= 1");
                table.CheckConstraint("CK_Episodes_Rating", "[Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 10)");
                table.ForeignKey("FK_Episodes_MediaItems_MediaItemId", x => x.MediaItemId, "MediaItems", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Episodes_MediaItemId_SeasonNumber_EpisodeNumber",
            table: "Episodes",
            columns: new[] { "MediaItemId", "SeasonNumber", "EpisodeNumber" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Episodes");
        migrationBuilder.DropTable(name: "MediaItems");
    }
}
