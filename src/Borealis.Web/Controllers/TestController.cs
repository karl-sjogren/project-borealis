using Borealis.Core.Contracts;
using Borealis.Core.Models;
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

    [HttpGet("discord")]
    public async Task<ActionResult> SendDiscordMessagesAsync(CancellationToken cancellationToken) {
#pragma warning disable RS0030 // Do not use banned APIs
        var player = new Player {
            Id = Guid.NewGuid(),
            ExternalId = 123456,
            State = 887,
            FurnaceLevel = 36,
            IsInAlliance = false,
            Name = "TestPlayer",
            IsMuted = false,
            PreviousNames = new List<PlayerNameHistoryEntry> {
                new PlayerNameHistoryEntry {
                    Name = "Zebra",
                    Timestamp = DateTime.UtcNow.AddDays(-1),
                },
                new PlayerNameHistoryEntry {
                    Name = "TestPlayer",
                    Timestamp = DateTime.UtcNow.AddDays(-2),
                },
                new PlayerNameHistoryEntry {
                    Name = "Giraffe",
                    Timestamp = DateTime.UtcNow.AddDays(-3),
                },
                new PlayerNameHistoryEntry {
                    Name = "OldName",
                    Timestamp = DateTime.UtcNow.AddDays(-4),
                }
            }
        };
#pragma warning restore RS0030 // Do not use banned APIs

        await _discordBotService.SendPlayerChangedNameMessageAsync(player, "NewName", "OldName", cancellationToken);
        await _discordBotService.SendPlayerChangedStateMessageAsync(player, 1, 2, cancellationToken);
        await _discordBotService.SendPlayerChangedFurnaceLevelMessageAsync(player, "10", cancellationToken);

#pragma warning disable RS0030 // Do not use banned APIs
        await _discordBotService.SendGiftCodeAddedMessageAsync(new GiftCode {
            Id = Guid.NewGuid(),
            Code = "TESTCODE",
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
#pragma warning restore RS0030 // Do not use banned APIs

        return Ok();
    }
}
