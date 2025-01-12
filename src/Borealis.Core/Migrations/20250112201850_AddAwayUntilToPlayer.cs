using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borealis.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAwayUntilToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AwayUntil",
                table: "Players",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayUntil",
                table: "Players");
        }
    }
}
