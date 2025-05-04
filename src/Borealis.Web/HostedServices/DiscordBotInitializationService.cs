
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Borealis.Web.HostedServices;

public class DiscordBotInitializationService : IHostedService {
    private readonly IDiscordClient _discordClient;
    private readonly InteractionService _interactionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscordBotInitializationService> _logger;

    public DiscordBotInitializationService(
            IDiscordClient discordClient,
            InteractionService interactionService,
            IConfiguration configuration,
            ILogger<DiscordBotInitializationService> logger) {
        _discordClient = discordClient;
        _interactionService = interactionService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        var token = _configuration["DiscordBotToken"] ?? throw new InvalidOperationException("DiscordBotToken is not set in the configuration.");

        var client = _discordClient as DiscordSocketClient;
        if(client == null) {
            throw new InvalidOperationException("DiscordClient is not a DiscordSocketClient.");
        }

        client.Log += message => {
            switch(message.Severity) {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    _logger.LogError(message.Exception, "Discord: {Message}", message.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(message.Exception, "Discord: {Message}", message.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(message.Exception, "Discord: {Message}", message.Message);
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    _logger.LogDebug(message.Exception, "Discord: {Message}", message.Message);
                    break;
            }

            return Task.CompletedTask;
        };

        client.Ready += async () => {
            _logger.LogInformation("Discord bot is ready.");

            await _interactionService.RegisterCommandsGloballyAsync();

            _logger.LogInformation("Discord commands registered.");
        };

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        var client = _discordClient as DiscordSocketClient;
        if(client == null) {
            throw new InvalidOperationException("DiscordClient is not a DiscordSocketClient.");
        }

        await client.LogoutAsync();
        await client.StopAsync();
    }
}
