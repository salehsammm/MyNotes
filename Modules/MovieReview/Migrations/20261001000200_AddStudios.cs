using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MovieReviews.Data;

#nullable disable

namespace MovieReviews.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261001000200_AddStudios")]
public partial class AddStudios : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Studios",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Studios", x => x.Id));

        migrationBuilder.Sql("INSERT INTO Studios (Name) SELECT DISTINCT LEFT(LTRIM(RTRIM(Studio)), 450) FROM Scenes WHERE NULLIF(LTRIM(RTRIM(Studio)), '') IS NOT NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Studios");
    }
}
