using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieReviews.Migrations
{
    /// <inheritdoc />
    public partial class AddScenePerformerBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Goddess",
                table: "ScenePerformers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "KindaPerfect",
                table: "ScenePerformers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TooGreat",
                table: "ScenePerformers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Goddess",
                table: "ScenePerformers");

            migrationBuilder.DropColumn(
                name: "KindaPerfect",
                table: "ScenePerformers");

            migrationBuilder.DropColumn(
                name: "TooGreat",
                table: "ScenePerformers");
        }
    }
}
