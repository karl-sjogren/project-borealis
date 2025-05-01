using Borealis.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("api/test")]
[Authorize(Roles = "AdminUser")]
public class TestController : Controller {
    private readonly IEnumerable<IGiftCodeScanner> _giftCodeScanners;
    private readonly IDiscordBotService _discordBotService;
    private readonly ILogger<TestController> _logger;

    public TestController(
            IEnumerable<IGiftCodeScanner> giftCodeScanners,
            IDiscordBotService discordBotService,
            ILogger<TestController> logger) {
        _giftCodeScanners = giftCodeScanners;
        _discordBotService = discordBotService;
        _logger = logger;
    }

    [HttpGet("gift-codes")]
    public async Task<ActionResult> ScanForGiftCodesAsync(CancellationToken cancellationToken) {
        var giftCodes = new List<string>();
        foreach(var giftCodeScanner in _giftCodeScanners) {
            var scannerGiftCodes = await giftCodeScanner.ScanGiftCodesAsync(cancellationToken);
            giftCodes.AddRange(scannerGiftCodes);
        }

        return Ok(giftCodes);
    }

    [HttpGet("discord/guilds")]
    public async Task<ActionResult> ListGuildsAsync(CancellationToken cancellationToken) {
        var guilds = await _discordBotService.GetGuildsAsync(cancellationToken);

        return Ok(guilds);
    }

    [HttpGet("discord/guilds/{guildId}/channels")]
    public async Task<ActionResult> ListChannelsAsync(ulong guildId, CancellationToken cancellationToken) {
        var channels = await _discordBotService.GetChannelsAsync(guildId, cancellationToken);

        return Ok(channels);
    }
}
