using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieReviews.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformerBodyFaceLegFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyQuality",
                table: "Performers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodySize",
                table: "Performers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaceQuality",
                table: "Performers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegQuality",
                table: "Performers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyQuality",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "BodySize",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "FaceQuality",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "LegQuality",
                table: "Performers");
        }
    }
}
