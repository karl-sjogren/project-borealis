using System.Globalization;
using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Core.Options;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Borealis.Core.Services;

public class DiscordBotService : IDiscordBotService {
    private readonly BorealisContext _borealisContext;
    private readonly IDiscordClient _discordClient;
    private readonly IOptions<BorealisOptions> _borealisOptions;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(
            BorealisContext borealisContext,
            IDiscordClient discordClient,
            IOptions<BorealisOptions> borealisOptions,
            ILogger<DiscordBotService> logger) {
        _borealisContext = borealisContext;
        _discordClient = discordClient;
        _borealisOptions = borealisOptions;
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

    private async Task SendPlayerMessageAsync(string? channelId, string message, Player player) {
        var options = _borealisOptions.Value;

        var builder = new ComponentBuilder();

        // TODO Add containers and stuff when supported

        if(!string.IsNullOrWhiteSpace(options.ApplicationUrl)) {
            builder.AddRow(new ActionRowBuilder()
                .WithButton("View player", style: ButtonStyle.Link, url: $"{options.ApplicationUrl}players/{player.Id}")
            );
        }

        await SendMessageAsync(channelId, message, builder.Build());
    }

    public async Task SendMessageAsync(string? channelId, string message, MessageComponent? messageComponent = null) {
        if(!ulong.TryParse(channelId, out var parseChannelId)) {
            _logger.LogWarning("Invalid channel ID: {ChannelId}", channelId);
            return;
        }

        var channel = await _discordClient.GetChannelAsync(parseChannelId) as ITextChannel;
        if(channel is null) {
            throw new InvalidOperationException($"Channel {parseChannelId} is not a valid message channel.");
        }

        await channel.SendMessageAsync(message, components: messageComponent);
    }

    public async Task SendGiftCodeAddedMessageAsync(GiftCode giftCode, CancellationToken cancellationToken) {
        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var options = _borealisOptions.Value;

        var builder = new ComponentBuilder();

        if(!string.IsNullOrWhiteSpace(options.ApplicationUrl)) {
            builder.AddRow(new ActionRowBuilder()
                .WithButton("View gift code", style: ButtonStyle.Link, url: $"{options.ApplicationUrl}gift-codes/{giftCode.Id}")
            );
        }

        var message = $"New gift code found: {giftCode.Code}";
        await SendMessageAsync(settings.GiftCodeChannelId, message, builder.Build());
    }

    public async Task SendPlayerChangedNameMessageAsync(Player player, string newName, string oldName, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return;
        }

        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var previousNames = player.PreviousNames
            .Select(x => x.Name)
            .Except([newName, oldName])
            .Distinct()
            .ToList();

        var message = $"Player {oldName} (#{player.State}) changed their name to {newName}.";
        if(previousNames.Count > 0) {
            message += $" Previous names: {string.Join(", ", previousNames)}.";
        }

        await SendPlayerMessageAsync(settings.PlayerRenameChannelId, message, player);
    }

    public async Task SendPlayerChangedFurnaceLevelMessageAsync(Player player, string furnaceLevel, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return;
        }

        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var message = $"Player {player.Name} (#{player.State}) increased their furnace level to {furnaceLevel}.";
        await SendPlayerMessageAsync(settings.PlayerFurnaceLevelChannelId, message, player);
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
        await SendPlayerMessageAsync(settings.PlayerMovedStateChannelId, message, player);
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
