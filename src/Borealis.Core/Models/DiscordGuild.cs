namespace Borealis.Core.Models;

public record DiscordGuild {
    public required string GuildId { get; init; }
    public required string Name { get; init; }
}
