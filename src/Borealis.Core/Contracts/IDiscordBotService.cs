using Borealis.Core.Models;

namespace Borealis.Core.Contracts;

public interface IDiscordBotService {
    Task SendGiftCodeAddedMessageAsync(GiftCode giftCode, CancellationToken cancellationToken);
    Task SendPlayerChangedNameMessageAsync(Player player, string newName, string oldName, CancellationToken cancellationToken);
    Task SendPlayerChangedFurnaceLevelMessageAsync(Player player, string furnaceLevel, CancellationToken cancellationToken);
    Task SendPlayerChangedStateMessageAsync(Player player, int newState, int oldState, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DiscordGuild>> GetGuildsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DiscordChannel>> GetChannelsAsync(string guildId, CancellationToken cancellationToken);
}
