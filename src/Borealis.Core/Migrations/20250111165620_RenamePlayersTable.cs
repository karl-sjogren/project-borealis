using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borealis.Core.Migrations
{
    /// <inheritdoc />
    public partial class RenamePlayersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_WhiteoutSurvivalPlayer_ExternalId",
                table: "WhiteoutSurvivalPlayer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhiteoutSurvivalPlayer",
                table: "WhiteoutSurvivalPlayer");

            migrationBuilder.RenameTable(
                name: "WhiteoutSurvivalPlayer",
                newName: "Players");

            migrationBuilder.RenameIndex(
                name: "IX_WhiteoutSurvivalPlayer_ExternalId",
                table: "Players",
                newName: "IX_Players_ExternalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Players_ExternalId",
                table: "Players",
                column: "ExternalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Players_ExternalId",
                table: "Players");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.RenameTable(
                name: "Players",
                newName: "WhiteoutSurvivalPlayer");

            migrationBuilder.RenameIndex(
                name: "IX_Players_ExternalId",
                table: "WhiteoutSurvivalPlayer",
                newName: "IX_WhiteoutSurvivalPlayer_ExternalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_WhiteoutSurvivalPlayer_ExternalId",
                table: "WhiteoutSurvivalPlayer",
                column: "ExternalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhiteoutSurvivalPlayer",
                table: "WhiteoutSurvivalPlayer",
                column: "Id");
        }
    }
}
