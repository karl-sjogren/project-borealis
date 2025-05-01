namespace Borealis.Core.Models;

public record DiscordChannel {
    public required ulong GuildId { get; init; }
    public required ulong ChannelId { get; init; }
    public required string Name { get; init; }
}
