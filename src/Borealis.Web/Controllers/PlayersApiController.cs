using Borealis.Core.Contracts;
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
        var result = await _playerService.GetByIdAsync(playerId, cancellationToken);

        if(!result.Success) {
            return NotFound();
        }

        var player = result.Data!;

        player.IsInAlliance = true;

        var updateResult = await _playerService.UpdateAsync(player, cancellationToken);

        if(!updateResult.Success) {
            return StatusCode(500);
        }

        return NoContent();
    }

    [HttpPut("{playerId:guid}/remove-from-alliance")]
    public async Task<ActionResult<PlayersIndexViewModel>> RemoveFromAllianceAsync(Guid playerId, CancellationToken cancellationToken) {
        var result = await _playerService.GetByIdAsync(playerId, cancellationToken);

        if(!result.Success) {
            return NotFound();
        }

        var player = result.Data!;

        player.IsInAlliance = false;

        var updateResult = await _playerService.UpdateAsync(player, cancellationToken);

        if(!updateResult.Success) {
            return StatusCode(500);
        }

        return NoContent();
    }
}
