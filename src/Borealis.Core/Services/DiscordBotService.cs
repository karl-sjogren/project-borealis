using System.Globalization;
using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Core.Options;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shorthand.Vite.Contracts;

namespace Borealis.Core.Services;

public class DiscordBotService : IDiscordBotService {
    private readonly BorealisContext _borealisContext;
    private readonly IDiscordClient _discordClient;
    private readonly IViteService _viteService;
    private readonly IOptions<BorealisOptions> _borealisOptions;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(
            BorealisContext borealisContext,
            IDiscordClient discordClient,
            IViteService viteService,
            IOptions<BorealisOptions> borealisOptions,
            ILogger<DiscordBotService> logger) {
        _borealisContext = borealisContext;
        _discordClient = discordClient;
        _viteService = viteService;
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

        var section = new SectionBuilder()
            .WithTextDisplay($"## {player.Name}")
            .WithTextDisplay($"**Level**: {player.FurnaceLevelString}, **State**: {player.State}")
            .WithTextDisplay($"**Last 5 names**: {string.Join(", ", player.PreviousNames.Reverse().Distinct().Take(5).Select(x => x.Name))}");

        if(player.HasFireCrystalFurnace && !string.IsNullOrWhiteSpace(options.ApplicationUrl)) {
            var badgeUrl = await _viteService.GetAssetUrlAsync($"assets/furnace-levels/{player.FurnaceLevelString.ToLowerInvariant()}.png");

            if(badgeUrl is not null) {
                var thumbnail = new ThumbnailBuilder()
                    .WithMedia($"{options.ApplicationUrl}/{badgeUrl}");

                section.WithAccessory(thumbnail);
            }
        }

        var builder = new ComponentBuilderV2()
            .WithTextDisplay(message)
            .WithSeparator(spacing: SeparatorSpacingSize.Large)
            .AddComponent(section);

        if(!string.IsNullOrWhiteSpace(options.ApplicationUrl)) {
            builder.AddComponent(new ActionRowBuilder()
                .WithButton("View player", style: ButtonStyle.Link, url: $"{options.ApplicationUrl}players/{player.Id}")
            );
        }

        await SendMessageAsync(channelId, builder.Build());
    }

    public async Task SendMessageAsync(string? channelId, MessageComponent? messageComponent = null) {
        if(!ulong.TryParse(channelId, out var parseChannelId)) {
            _logger.LogWarning("Invalid channel ID: {ChannelId}", channelId);
            return;
        }

        var channel = await _discordClient.GetChannelAsync(parseChannelId) as ITextChannel;
        if(channel is null) {
            throw new InvalidOperationException($"Channel {parseChannelId} is not a valid message channel.");
        }

        await channel.SendMessageAsync(components: messageComponent);
    }

    public async Task SendGiftCodeAddedMessageAsync(GiftCode giftCode, CancellationToken cancellationToken) {
        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var options = _borealisOptions.Value;

        var builder = new ComponentBuilderV2()
            .WithTextDisplay($"New gift code found: **{giftCode.Code}**");

        if(!string.IsNullOrWhiteSpace(options.ApplicationUrl)) {
            builder.AddComponent(new ActionRowBuilder()
                .WithButton("View gift code", style: ButtonStyle.Link, url: $"{options.ApplicationUrl}gift-codes/{giftCode.Id}")
            );
        }

        await SendMessageAsync(settings.GiftCodeChannelId, builder.Build());
    }

    public async Task SendPlayerChangedNameMessageAsync(Player player, string newName, string oldName, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return;
        }

        var settings = await GetSettingsAsync(cancellationToken);
        if(settings is null) {
            return;
        }

        var message = $"Player **{oldName}** (#{player.State}) changed their name to **{newName}**.";

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

        var message = $"Player **{player.Name}** (#{player.State}) increased their furnace level to **{furnaceLevel}**.";
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

        var message = $"Player **{player.Name}** moved state from **{oldState}** to **{newState}**.";
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
