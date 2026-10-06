using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MovieReviews.Data;

#nullable disable

namespace MovieReviews.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261006000100_AddSceneSourceMetadata")]
public partial class AddSceneSourceMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var name in new[] { "Url", "AlternateUrls", "CoverUrl", "RaindropId", "SourceTitle", "SourceNote", "SourceExcerpt", "SourceHighlights", "SourceArchive", "SourceData" })
            migrationBuilder.AddColumn<string>(name: name, table: "Scenes", type: "nvarchar(max)", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "SourceCreated", table: "Scenes", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<bool>(name: "SourceFavorite", table: "Scenes", type: "bit", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var name in new[] { "Url", "AlternateUrls", "CoverUrl", "RaindropId", "SourceTitle", "SourceNote", "SourceExcerpt", "SourceHighlights", "SourceArchive", "SourceData", "SourceCreated", "SourceFavorite" })
            migrationBuilder.DropColumn(name: name, table: "Scenes");
    }
}
