using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MovieReviews.Data;

#nullable disable

namespace MovieReviews.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261001000300_AddReviewPlaybackAndPerformerDetails")]
public partial class AddReviewPlaybackAndPerformerDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "ResumeAtSeconds", table: "Scenes", type: "int", nullable: true);
        migrationBuilder.AddColumn<bool>(name: "SetupSpeaksNoEnglish", table: "Scenes", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>(name: "AgeCategory", table: "Performers", type: "int", nullable: true);
        migrationBuilder.AddColumn<string>(name: "BellyQuality", table: "Performers", type: "nvarchar(max)", nullable: true);
        migrationBuilder.AddColumn<bool>(name: "DoesNotSpeakEnglish", table: "Performers", type: "bit", nullable: false, defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ResumeAtSeconds", table: "Scenes");
        migrationBuilder.DropColumn(name: "SetupSpeaksNoEnglish", table: "Scenes");
        migrationBuilder.DropColumn(name: "AgeCategory", table: "Performers");
        migrationBuilder.DropColumn(name: "BellyQuality", table: "Performers");
        migrationBuilder.DropColumn(name: "DoesNotSpeakEnglish", table: "Performers");
    }
}
