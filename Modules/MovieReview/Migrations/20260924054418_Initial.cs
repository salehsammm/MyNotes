using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieReviews.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Performers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Skin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoobsSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoobsQuality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssQuality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bj = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Moans = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsWatchlisted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Performers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Studio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallRating = table.Column<int>(type: "int", nullable: false),
                    OverallModifier = table.Column<int>(type: "int", nullable: false),
                    Verdict = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetupRating = table.Column<int>(type: "int", nullable: false),
                    SetupNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SexRating = table.Column<int>(type: "int", nullable: false),
                    SexNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPov = table.Column<bool>(type: "bit", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Positions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Locations = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScenePerformers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SceneId = table.Column<int>(type: "int", nullable: false),
                    PerformerId = table.Column<int>(type: "int", nullable: false),
                    SkinOverride = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoobsOverride = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssOverride = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BjOverride = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoansOverride = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenePerformers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenePerformers_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScenePerformers_Scenes_SceneId",
                        column: x => x.SceneId,
                        principalTable: "Scenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScenePerformers_PerformerId",
                table: "ScenePerformers",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenePerformers_SceneId",
                table: "ScenePerformers",
                column: "SceneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScenePerformers");

            migrationBuilder.DropTable(
                name: "Performers");

            migrationBuilder.DropTable(
                name: "Scenes");
        }
    }
}
