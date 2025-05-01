using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Discord;
using Microsoft.Extensions.Configuration;

namespace Borealis.Core.Services;

public class DiscordBotService : IDiscordBotService {
    private readonly IDiscordClient _discordClient;
    private readonly IConfiguration _configuration;

    public DiscordBotService(IDiscordClient discordClient, IConfiguration configuration) {
        _discordClient = discordClient;
        _configuration = configuration;
    }

    private ulong GetChannelId() {
        var channelId = _configuration["DiscordBotChannelId"];
        if(channelId == null) {
            throw new InvalidOperationException("DiscordBotChannelId is not set in the configuration.");
        }

        if(!ulong.TryParse(channelId, out var result)) {
            throw new InvalidOperationException("DiscordBotChannelId is not a valid number.");
        }

        return result;
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken) {
        var channelId = GetChannelId();
        var channel = await _discordClient.GetChannelAsync(channelId) as IMessageChannel;
        if(channel is null) {
            throw new InvalidOperationException($"Channel {channelId} is not a valid message channel.");
        }

        await channel.SendMessageAsync(message);
    }

    public Task SendGiftCodeAddedMessageAsync(GiftCode giftCode, CancellationToken cancellationToken) {
        var message = $"New gift code found: {giftCode.Code}";
        return SendMessageAsync(message, cancellationToken);
    }

    public Task SendPlayerChangedNameMessageAsync(Player player, string newName, string oldName, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return Task.CompletedTask;
        }

        var message = $"Player {oldName} changed their name to {newName}.";
        return SendMessageAsync(message, cancellationToken);
    }

    public Task SendPlayerChangedFurnaceLevelMessageAsync(Player player, string furnaceLevel, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return Task.CompletedTask;
        }

        var message = $"Player {player.Name} increased their furnace level to {furnaceLevel}.";
        return SendMessageAsync(message, cancellationToken);
    }

    public Task SendPlayerChangedStateMessageAsync(Player player, int newState, int oldState, CancellationToken cancellationToken) {
        if(player.IsMuted) {
            return Task.CompletedTask;
        }

        var message = $"Player {player.Name} moved state from {oldState} to {newState}.";
        return SendMessageAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DiscordGuild>> GetGuildsAsync(CancellationToken cancellationToken) {
        var guilds = await _discordClient.GetGuildsAsync();

        return [.. guilds.Select(guild => new DiscordGuild {
            GuildId = guild.Id,
            Name = guild.Name
        })];
    }

    public async Task<IReadOnlyCollection<DiscordChannel>> GetChannelsAsync(ulong guildId, CancellationToken cancellationToken) {
        var guild = await _discordClient.GetGuildAsync(guildId);
        var channels = await guild.GetChannelsAsync();

        return [.. channels.Select(channel => new DiscordChannel {
            GuildId = channel.GuildId,
            ChannelId = channel.Id,
            Name = channel.Name
        })];
    }
}
