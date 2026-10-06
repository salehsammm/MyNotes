using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieReviews.Migrations
{
    /// <inheritdoc />
    public partial class Lookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssSize",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "Bj",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "BoobsSize",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "Skin",
                table: "Performers");

            migrationBuilder.AddColumn<int>(
                name: "AssSizeId",
                table: "Performers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoobSizeId",
                table: "Performers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkinColorId",
                table: "Performers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssSize",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssSize", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BjTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BjTag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoobSize",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoobSize", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkinColor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinColor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformerBjTags",
                columns: table => new
                {
                    BjTagsId = table.Column<int>(type: "int", nullable: false),
                    PerformersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformerBjTags", x => new { x.BjTagsId, x.PerformersId });
                    table.ForeignKey(
                        name: "FK_PerformerBjTags_BjTag_BjTagsId",
                        column: x => x.BjTagsId,
                        principalTable: "BjTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformerBjTags_Performers_PerformersId",
                        column: x => x.PerformersId,
                        principalTable: "Performers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Performers_AssSizeId",
                table: "Performers",
                column: "AssSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_BoobSizeId",
                table: "Performers",
                column: "BoobSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_SkinColorId",
                table: "Performers",
                column: "SkinColorId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformerBjTags_PerformersId",
                table: "PerformerBjTags",
                column: "PerformersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Performers_AssSize_AssSizeId",
                table: "Performers",
                column: "AssSizeId",
                principalTable: "AssSize",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Performers_BoobSize_BoobSizeId",
                table: "Performers",
                column: "BoobSizeId",
                principalTable: "BoobSize",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Performers_SkinColor_SkinColorId",
                table: "Performers",
                column: "SkinColorId",
                principalTable: "SkinColor",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Performers_AssSize_AssSizeId",
                table: "Performers");

            migrationBuilder.DropForeignKey(
                name: "FK_Performers_BoobSize_BoobSizeId",
                table: "Performers");

            migrationBuilder.DropForeignKey(
                name: "FK_Performers_SkinColor_SkinColorId",
                table: "Performers");

            migrationBuilder.DropTable(
                name: "AssSize");

            migrationBuilder.DropTable(
                name: "BoobSize");

            migrationBuilder.DropTable(
                name: "PerformerBjTags");

            migrationBuilder.DropTable(
                name: "SkinColor");

            migrationBuilder.DropTable(
                name: "BjTag");

            migrationBuilder.DropIndex(
                name: "IX_Performers_AssSizeId",
                table: "Performers");

            migrationBuilder.DropIndex(
                name: "IX_Performers_BoobSizeId",
                table: "Performers");

            migrationBuilder.DropIndex(
                name: "IX_Performers_SkinColorId",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "AssSizeId",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "BoobSizeId",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "SkinColorId",
                table: "Performers");

            migrationBuilder.AddColumn<string>(
                name: "AssSize",
                table: "Performers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bj",
                table: "Performers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BoobsSize",
                table: "Performers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skin",
                table: "Performers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
