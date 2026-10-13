using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotes.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteGroupsAndResearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoteGroupId",
                table: "PersonalNotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NoteGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoteResearch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalNoteId = table.Column<int>(type: "int", nullable: false),
                    SourceKind = table.Column<int>(type: "int", nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteResearch", x => x.Id);
                    table.CheckConstraint("CK_NoteResearch_SourceKind", "[SourceKind] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_NoteResearch_PersonalNotes_PersonalNoteId",
                        column: x => x.PersonalNoteId,
                        principalTable: "PersonalNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalNotes_NoteGroupId",
                table: "PersonalNotes",
                column: "NoteGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteGroups_Category",
                table: "NoteGroups",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_NoteResearch_PersonalNoteId_Position",
                table: "NoteResearch",
                columns: new[] { "PersonalNoteId", "Position" });

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalNotes_NoteGroups_NoteGroupId",
                table: "PersonalNotes",
                column: "NoteGroupId",
                principalTable: "NoteGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalNotes_NoteGroups_NoteGroupId",
                table: "PersonalNotes");

            migrationBuilder.DropTable(
                name: "NoteGroups");

            migrationBuilder.DropTable(
                name: "NoteResearch");

            migrationBuilder.DropIndex(
                name: "IX_PersonalNotes_NoteGroupId",
                table: "PersonalNotes");

            migrationBuilder.DropColumn(
                name: "NoteGroupId",
                table: "PersonalNotes");
        }
    }
}
