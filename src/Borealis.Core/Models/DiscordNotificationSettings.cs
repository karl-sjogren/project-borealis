namespace Borealis.Core.Models;

public class DiscordNotificationSettings : EntityBase {
    public string? GuildId { get; set; }
    public string? GiftCodeChannelId { get; set; }
    public string? PlayerRenameChannelId { get; set; }
    public string? PlayerFurnaceLevelChannelId { get; set; }
    public string? PlayerMovedStateChannelId { get; set; }
}
