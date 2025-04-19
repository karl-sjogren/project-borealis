using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("api/players")]
[Authorize(Roles = "TrustedUser")]
public class PlayersAPIController : Controller {
    private readonly IPlayerService _playerService;
    private readonly ILogger<PlayersAPIController> _logger;

    public PlayersAPIController(IPlayerService playerService, ILogger<PlayersAPIController> logger) {
        _playerService = playerService;
        _logger = logger;
    }

    [HttpDelete("{playerId:guid}")]
    public async Task<ActionResult<PlayersIndexViewModel>> DeleteAsync(Guid playerId, CancellationToken cancellationToken) {
        var result = await _playerService.DeleteAsync(playerId, cancellationToken);

        if(!result.Success) {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{playerId:guid}/add-to-alliance")]
    public async Task<ActionResult<PlayersIndexViewModel>> AddToAllianceAsync(Guid playerId, CancellationToken cancellationToken) {
        return await UpdatePlayerActionAsync(playerId, player => player.IsInAlliance = true, cancellationToken);
    }

    [HttpPut("{playerId:guid}/remove-from-alliance")]
    public async Task<ActionResult<PlayersIndexViewModel>> RemoveFromAllianceAsync(Guid playerId, CancellationToken cancellationToken) {
        return await UpdatePlayerActionAsync(playerId, player => player.IsInAlliance = false, cancellationToken);
    }

    [HttpPut("{playerId:guid}/mute")]
    public async Task<ActionResult<PlayersIndexViewModel>> MuteAsync(Guid playerId, CancellationToken cancellationToken) {
        return await UpdatePlayerActionAsync(playerId, player => player.IsMuted = true, cancellationToken);
    }

    [HttpPut("{playerId:guid}/unmute")]
    public async Task<ActionResult<PlayersIndexViewModel>> UnmuteAsync(Guid playerId, CancellationToken cancellationToken) {
        return await UpdatePlayerActionAsync(playerId, player => player.IsMuted = false, cancellationToken);
    }

    [HttpPut("{playerId:guid}/set-force-redeem-gift-codes")]
    public async Task<ActionResult<PlayersIndexViewModel>> SetForceRedeemGiftCodesAsync(Guid playerId, CancellationToken cancellationToken) {
        return await UpdatePlayerActionAsync(playerId, player => player.ForceRedeemGiftCodes = true, cancellationToken);
    }

    [HttpPut("{playerId:guid}/unset-force-redeem-gift-codes")]
    public async Task<ActionResult<PlayersIndexViewModel>> UnsetForceRedeemGiftCodesAsync(Guid playerId, CancellationToken cancellationToken) {
        return await UpdatePlayerActionAsync(playerId, player => player.ForceRedeemGiftCodes = false, cancellationToken);
    }

    private async Task<ActionResult<PlayersIndexViewModel>> UpdatePlayerActionAsync(Guid playerId, Action<Player> updateAction, CancellationToken cancellationToken) {
        var result = await _playerService.GetByIdAsync(playerId, cancellationToken);

        if(!result.Success || result.Data is null) {
            return NotFound();
        }

        var player = result.Data;

        updateAction(player);

        var updateResult = await _playerService.UpdateAsync(player, cancellationToken);

        if(!updateResult.Success) {
            return StatusCode(500);
        }

        return NoContent();
    }
}
