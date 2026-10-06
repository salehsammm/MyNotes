using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MyNotes.Data;

#nullable disable

namespace MyNotes.Migrations;

[DbContext(typeof(MediaDbContext))]
[Migration("20261006203000_ExternalSiteFlagsAndSpecialEpisodes")]
public class ExternalSiteFlagsAndSpecialEpisodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(name: "SavedToImdb", table: "MediaItems", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(name: "SavedToLetterboxd", table: "MediaItems", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.DropCheckConstraint(name: "CK_Episodes_Numbers", table: "Episodes");
        migrationBuilder.AddCheckConstraint(name: "CK_Episodes_Numbers", table: "Episodes", sql: "[SeasonNumber] >= 1 AND [EpisodeNumber] >= 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(name: "CK_Episodes_Numbers", table: "Episodes");
        migrationBuilder.AddCheckConstraint(name: "CK_Episodes_Numbers", table: "Episodes", sql: "[SeasonNumber] >= 1 AND [EpisodeNumber] >= 1");
        migrationBuilder.DropColumn(name: "SavedToImdb", table: "MediaItems");
        migrationBuilder.DropColumn(name: "SavedToLetterboxd", table: "MediaItems");
    }
}
