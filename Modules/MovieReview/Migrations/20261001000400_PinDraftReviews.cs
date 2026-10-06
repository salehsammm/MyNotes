using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MovieReviews.Data;

#nullable disable

namespace MovieReviews.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261001000400_PinDraftReviews")]
public partial class PinDraftReviews : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsPinnedDraft", table: "Scenes", type: "bit", nullable: false, defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsPinnedDraft", table: "Scenes");
    }
}
