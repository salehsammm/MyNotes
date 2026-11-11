using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotes.Migrations
{
    /// <inheritdoc />
    public partial class LimitDiaryToOneEntryPerDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_PersonalNotes_DiaryDate",
                table: "PersonalNotes",
                column: "NoteDate",
                unique: true,
                filter: "[Kind] = 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PersonalNotes_DiaryDate",
                table: "PersonalNotes");
        }
    }
}
