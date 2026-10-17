using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotes.Modules.MovieReview.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchiveName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OriginalArchive = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ImportedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArchiveCollectionId = table.Column<int>(type: "int", nullable: false),
                    SourceRow = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kind = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttachmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Progress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SceneId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveItems_ArchiveCollections_ArchiveCollectionId",
                        column: x => x.ArchiveCollectionId,
                        principalTable: "ArchiveCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArchiveItems_Scenes_SceneId",
                        column: x => x.SceneId,
                        principalTable: "Scenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveCollections_Sha256",
                table: "ArchiveCollections",
                column: "Sha256",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveItems_ArchiveCollectionId_SourceRow",
                table: "ArchiveItems",
                columns: new[] { "ArchiveCollectionId", "SourceRow" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveItems_SceneId",
                table: "ArchiveItems",
                column: "SceneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveItems");

            migrationBuilder.DropTable(
                name: "ArchiveCollections");
        }
    }
}
