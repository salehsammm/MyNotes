using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotes.Modules.MovieReview.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistSceneOrderAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "PlaylistScenes",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "PlaylistScenes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Preserve the page's previous title order for existing playlist entries.
            migrationBuilder.Sql(@";WITH Ordered AS (
                SELECT ps.PlaylistId, ps.SceneId,
                    ROW_NUMBER() OVER (PARTITION BY ps.PlaylistId ORDER BY s.Title, ps.SceneId) AS NewPosition
                FROM PlaylistScenes ps JOIN Scenes s ON s.Id = ps.SceneId
            )
            UPDATE ps SET Position = CAST(o.NewPosition AS int)
            FROM PlaylistScenes ps JOIN Ordered o
                ON o.PlaylistId = ps.PlaylistId AND o.SceneId = ps.SceneId;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PlaylistScene_Position",
                table: "PlaylistScenes",
                sql: "[Position] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PlaylistScene_Position",
                table: "PlaylistScenes");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "PlaylistScenes");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "PlaylistScenes");
        }
    }
}
