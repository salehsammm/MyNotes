using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookmarkCounter.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyIntervals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FirstDueDate",
                table: "ChecklistItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatEveryDays",
                table: "ChecklistItems",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstDueDate",
                table: "ChecklistItems");

            migrationBuilder.DropColumn(
                name: "RepeatEveryDays",
                table: "ChecklistItems");
        }
    }
}
