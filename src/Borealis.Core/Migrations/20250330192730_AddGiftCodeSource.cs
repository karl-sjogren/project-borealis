using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borealis.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftCodeSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "GiftCodes",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "GiftCodes");
        }
    }
}
