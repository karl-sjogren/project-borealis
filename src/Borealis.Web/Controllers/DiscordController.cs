using Borealis.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("api/discord")]
[Authorize(Roles = "AdminUser")]
public class DiscordController : Controller {
    private readonly IDiscordBotService _discordBotService;
    private readonly ILogger<DiscordController> _logger;

    public DiscordController(
            IDiscordBotService discordBotService,
            ILogger<DiscordController> logger) {
        _discordBotService = discordBotService;
        _logger = logger;
    }

    [HttpGet("guilds")]
    public async Task<ActionResult> ListGuildsAsync(CancellationToken cancellationToken) {
        var guilds = await _discordBotService.GetGuildsAsync(cancellationToken);

        return Ok(guilds);
    }

    [HttpGet("guilds/{guildId}/channels")]
    public async Task<ActionResult> ListChannelsAsync(ulong guildId, CancellationToken cancellationToken) {
        var channels = await _discordBotService.GetChannelsAsync(guildId, cancellationToken);

        return Ok(channels);
    }
}
