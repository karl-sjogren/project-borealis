namespace Borealis.Core.Models;

public record DiscordChannel {
    public required string GuildId { get; init; }
    public required string ChannelId { get; init; }
    public required string Name { get; init; }
}
