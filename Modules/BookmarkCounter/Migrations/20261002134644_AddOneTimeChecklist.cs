using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookmarkCounter.Migrations
{
    /// <inheritdoc />
    public partial class AddOneTimeChecklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOneTime",
                table: "ChecklistItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ScheduledPeriodStart",
                table: "ChecklistItems",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOneTime",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "ScheduledPeriodStart",
                table: "ChecklistItems");
        }
    }
}
