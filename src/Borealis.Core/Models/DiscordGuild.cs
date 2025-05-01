namespace Borealis.Core.Models;

public record DiscordGuild {
    public required ulong GuildId { get; init; }
    public required string Name { get; init; }
}
