using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MovieReviews.Data;

#nullable disable

namespace MovieReviews.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261001000100_ArchivePerformers")]
public partial class ArchivePerformers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsArchived", table: "Performers", type: "bit", nullable: false, defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsArchived", table: "Performers");
    }
}
