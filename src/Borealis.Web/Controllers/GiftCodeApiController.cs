using Borealis.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("api/gift-codes")]
[Authorize(Roles = "TrustedUser")]
public class GiftCodeApiController : Controller {
    private readonly IGiftCodeService _giftCodeService;
    private readonly ILogger<GiftCodeApiController> _logger;

    public GiftCodeApiController(IGiftCodeService giftCodeService, ILogger<GiftCodeApiController> logger) {
        _giftCodeService = giftCodeService;
        _logger = logger;
    }

    [HttpPut("{giftCodeId:guid}/redeem")]
    public async Task<ActionResult> EnqueueGiftCodeAsync(Guid giftCodeId, CancellationToken cancellationToken) {
        var result = await _giftCodeService.EnqueueGiftCodeAsync(giftCodeId, cancellationToken);

        if(!result.Success) {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{giftCodeId:guid}")]
    public async Task<ActionResult> DeleteAsync(Guid giftCodeId, CancellationToken cancellationToken) {
        var result = await _giftCodeService.DeleteAsync(giftCodeId, cancellationToken);

        if(!result.Success) {
            return NotFound();
        }

        return NoContent();
    }
}
