using System.Globalization;
using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core.Services;

public class DiscordBotService : IDiscordBotService {
    private readonly BorealisContext _borealisContext;
    private readonly IDiscordClient _discordClient;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(BorealisContext borealisContext, IDiscordClient discordClient, ILogger<DiscordBotService> logger) {
        _borealisContext = borealisContext;
        _discordClient = discordClient;
        _logger = logger;
    }

    private async Task<DiscordNotificationSettings?> GetSettingsAsync(CancellationToken cancellationToken) {
        var settings = await _borealisContext
            .DiscordNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if(settings is null) {
            _logger.LogWarning("Discord notification settings not found. Setup required from web interface.");
        }

        return settings;
    }

    public async Task SendMessageAsync(string? channelId, string message, CancellationToken cancellationToken) {
        if(!ulong.TryParse(channelId, out var parseChannelId)) {
            _logger.LogWarning("Invalid channel ID: {ChannelId}", channelId);
            return;
        }

        var channel = await _discordClient.GetChannelAsync(parseChannelId) as ITextChannel;
        if(channel is null) {
            throw new InvalidOperationException($"Channel {parseChannelId} is not a valid message channel.");
        }

        await channel.SendMessageAsync(message);
    }

    public async Task SendGiftCodeAddedMessageAsync(GiftCode giftCode, CancellationToken cancellationToken) {
        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var message = $"New gift code found: {giftCode.Code}";
        await SendMessageAsync(settings.GiftCodeChannelId, message, cancellationToken);
    }

    public async Task SendPlayerChangedNameMessageAsync(Player player, string newName, string oldName, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return;
        }

        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var message = $"Player {oldName} changed their name to {newName}.";
        await SendMessageAsync(settings.PlayerRenameChannelId, message, cancellationToken);
    }

    public async Task SendPlayerChangedFurnaceLevelMessageAsync(Player player, string furnaceLevel, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return;
        }

        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var message = $"Player {player.Name} increased their furnace level to {furnaceLevel}.";
        await SendMessageAsync(settings.PlayerFurnaceLevelChannelId, message, cancellationToken);
    }

    public async Task SendPlayerChangedStateMessageAsync(Player player, int newState, int oldState, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return;
        }

        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var message = $"Player {player.Name} moved state from {oldState} to {newState}.";
        await SendMessageAsync(settings.PlayerMovedStateChannelId, message, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DiscordGuild>> GetGuildsAsync(CancellationToken cancellationToken) {
        var guilds = await _discordClient.GetGuildsAsync();

        return [.. guilds
            .Select(guild => new DiscordGuild {
                GuildId = guild.Id.ToString(CultureInfo.InvariantCulture),
                Name = guild.Name
            })
            .OrderBy(guild => guild.Name, StringComparer.OrdinalIgnoreCase)
        ];
    }

    public async Task<IReadOnlyCollection<DiscordChannel>> GetChannelsAsync(string guildId, CancellationToken cancellationToken) {
        var guild = await _discordClient.GetGuildAsync(ulong.Parse(guildId, CultureInfo.InvariantCulture));
        var channels = await guild.GetChannelsAsync();

        return [.. channels
            .Where(channel => channel is ITextChannel)
            .Select(channel => new DiscordChannel {
                GuildId = channel.GuildId.ToString(CultureInfo.InvariantCulture),
                ChannelId = channel.Id.ToString(CultureInfo.InvariantCulture),
                Name = channel.Name
            })
            .OrderBy(guild => guild.Name, StringComparer.OrdinalIgnoreCase)
        ];
    }
}
