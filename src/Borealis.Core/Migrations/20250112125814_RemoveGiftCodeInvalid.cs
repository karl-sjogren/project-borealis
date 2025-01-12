using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borealis.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGiftCodeInvalid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInvalid",
                table: "GiftCodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInvalid",
                table: "GiftCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
