using Borealis.Core.Contracts;
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
}
