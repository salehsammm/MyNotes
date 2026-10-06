using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MovieReviews.Data;

#nullable disable

namespace MovieReviews.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261001000000_AddSceneDrafts")]
public partial class AddSceneDrafts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDraft", table: "Scenes", type: "bit", nullable: false, defaultValue: false);

        migrationBuilder.AlterColumn<int>(
            name: "OverallRating", table: "Scenes", type: "int", nullable: true,
            oldClrType: typeof(int), oldType: "int");
        migrationBuilder.AlterColumn<int>(
            name: "SetupRating", table: "Scenes", type: "int", nullable: true,
            oldClrType: typeof(int), oldType: "int");
        migrationBuilder.AlterColumn<int>(
            name: "SexRating", table: "Scenes", type: "int", nullable: true,
            oldClrType: typeof(int), oldType: "int");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE Scenes SET OverallRating = 2 WHERE OverallRating IS NULL; UPDATE Scenes SET SetupRating = 2 WHERE SetupRating IS NULL; UPDATE Scenes SET SexRating = 2 WHERE SexRating IS NULL;");
        migrationBuilder.AlterColumn<int>(
            name: "OverallRating", table: "Scenes", type: "int", nullable: false,
            oldClrType: typeof(int), oldType: "int", oldNullable: true);
        migrationBuilder.AlterColumn<int>(
            name: "SetupRating", table: "Scenes", type: "int", nullable: false,
            oldClrType: typeof(int), oldType: "int", oldNullable: true);
        migrationBuilder.AlterColumn<int>(
            name: "SexRating", table: "Scenes", type: "int", nullable: false,
            oldClrType: typeof(int), oldType: "int", oldNullable: true);
        migrationBuilder.DropColumn(name: "IsDraft", table: "Scenes");
    }
}
