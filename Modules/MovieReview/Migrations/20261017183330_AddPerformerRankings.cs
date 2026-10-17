using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotes.Modules.MovieReview.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformerRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RankingPosition",
                table: "Performers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerformerId",
                table: "ArchiveItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Performer_RankingPosition",
                table: "Performers",
                sql: "[RankingPosition] IS NULL OR [RankingPosition] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveItems_PerformerId",
                table: "ArchiveItems",
                column: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveItems_Performers_PerformerId",
                table: "ArchiveItems",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveItems_Performers_PerformerId",
                table: "ArchiveItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Performer_RankingPosition",
                table: "Performers");

            migrationBuilder.DropIndex(
                name: "IX_ArchiveItems_PerformerId",
                table: "ArchiveItems");

            migrationBuilder.DropColumn(
                name: "RankingPosition",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "PerformerId",
                table: "ArchiveItems");
        }
    }
}
