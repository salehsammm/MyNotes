using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotes.Modules.BookmarkCounter.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAppearance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "ChecklistItems",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "ChecklistItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "ChecklistItems");
        }
    }
}
