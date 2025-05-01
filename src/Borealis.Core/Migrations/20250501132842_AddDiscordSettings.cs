using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borealis.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordNotificationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GuildId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    GiftCodeChannelId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PlayerRenameChannelId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PlayerFurnaceLevelChannelId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PlayerMovedStateChannelId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordNotificationSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordNotificationSettings");
        }
    }
}
