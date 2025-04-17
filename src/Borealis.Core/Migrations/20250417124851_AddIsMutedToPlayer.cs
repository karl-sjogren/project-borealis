using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borealis.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddIsMutedToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMuted",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMuted",
                table: "Players");
        }
    }
}
