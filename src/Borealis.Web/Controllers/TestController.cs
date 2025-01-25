using Borealis.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("api/test")]
[Authorize(Roles = "AdminUser")]
public class TestController : Controller {
    private readonly IEnumerable<IGiftCodeScanner> _giftCodeScanners;
    private readonly ILogger<TestController> _logger;

    public TestController(IEnumerable<IGiftCodeScanner> giftCodeScanners, ILogger<TestController> logger) {
        _giftCodeScanners = giftCodeScanners;
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
}
