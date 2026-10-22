using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotes.Modules.MovieReview.Migrations
{
    /// <inheritdoc />
    public partial class AddGuidePictures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuidePictures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuideSectionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuidePictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuidePictures_GuideSections_GuideSectionId",
                        column: x => x.GuideSectionId,
                        principalTable: "GuideSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuidePictures_GuideSectionId",
                table: "GuidePictures",
                column: "GuideSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuidePictures");
        }
    }
}
