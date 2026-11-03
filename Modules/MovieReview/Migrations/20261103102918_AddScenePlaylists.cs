using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotes.Modules.MovieReview.Migrations
{
    /// <inheritdoc />
    public partial class AddScenePlaylists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistScenes",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "int", nullable: false),
                    SceneId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistScenes", x => new { x.PlaylistId, x.SceneId });
                    table.ForeignKey(
                        name: "FK_PlaylistScenes_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistScenes_Scenes_SceneId",
                        column: x => x.SceneId,
                        principalTable: "Scenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_Name",
                table: "Playlists",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistScenes_SceneId",
                table: "PlaylistScenes",
                column: "SceneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaylistScenes");

            migrationBuilder.DropTable(
                name: "Playlists");
        }
    }
}
