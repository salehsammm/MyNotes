using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MovieReviews.Data;

#nullable disable

namespace MovieReviews.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261011000100_AddSetupFlags")]
public partial class AddSetupFlags : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(name: "SetupWeird", table: "Scenes", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(name: "SetupDumb", table: "Scenes", type: "bit", nullable: false, defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "SetupWeird", table: "Scenes");
        migrationBuilder.DropColumn(name: "SetupDumb", table: "Scenes");
    }
}
