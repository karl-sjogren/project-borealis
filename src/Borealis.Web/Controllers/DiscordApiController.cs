using Borealis.Core;
using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Web.Controllers;

[Route("api/discord")]
[Authorize(Roles = "AdminUser")]
public class DiscordApiController : Controller {
    private readonly BorealisContext _borealisContext;
    private readonly IDiscordBotService _discordBotService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DiscordApiController> _logger;

    public DiscordApiController(
            BorealisContext borealisContext,
            IDiscordBotService discordBotService,
            TimeProvider timeProvider,
            ILogger<DiscordApiController> logger) {
        _borealisContext = borealisContext;
        _discordBotService = discordBotService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    [HttpPut("settings")]
    public async Task<ActionResult> StoreSettingsAsync([FromForm] DiscordNotificationSettings settings, CancellationToken cancellationToken) {
        var existingSettings = await _borealisContext
            .DiscordNotificationSettings
            .FirstOrDefaultAsync(cancellationToken);

        if(existingSettings is null) {
            existingSettings = new DiscordNotificationSettings {
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _borealisContext.DiscordNotificationSettings.Add(existingSettings);
        }

        existingSettings.GuildId = settings.GuildId;
        existingSettings.GiftCodeChannelId = settings.GiftCodeChannelId;
        existingSettings.PlayerFurnaceLevelChannelId = settings.PlayerFurnaceLevelChannelId;
        existingSettings.PlayerMovedStateChannelId = settings.PlayerMovedStateChannelId;
        existingSettings.PlayerRenameChannelId = settings.PlayerRenameChannelId;

        if(_borealisContext.Entry(existingSettings).State == EntityState.Modified) {
            existingSettings.UpdatedAt = _timeProvider.GetUtcNow();
        }

        await _borealisContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("guilds")]
    public async Task<ActionResult> ListGuildsAsync(CancellationToken cancellationToken) {
        var guilds = await _discordBotService.GetGuildsAsync(cancellationToken);

        return Ok(guilds);
    }

    [HttpGet("guilds/{guildId}/channels")]
    public async Task<ActionResult> ListChannelsAsync(string guildId, CancellationToken cancellationToken) {
        var channels = await _discordBotService.GetChannelsAsync(guildId, cancellationToken);

        return Ok(channels);
    }
}
