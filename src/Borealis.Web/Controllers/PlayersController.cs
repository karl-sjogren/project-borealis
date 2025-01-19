using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Core.Requests;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("players")]
[Authorize(Roles = "TrustedUser")]
public class PlayersController : Controller {
    private readonly IPlayerService _playerService;
    private readonly IGiftCodeService _giftCodeService;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(IPlayerService playerService, IGiftCodeService giftCodeService, ILogger<PlayersController> logger) {
        _playerService = playerService;
        _giftCodeService = giftCodeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PlayersIndexViewModel>> IndexAsync([FromQuery] PlayerQuery playerQuery, CancellationToken cancellationToken) {
        var result = await _playerService.GetPagedAsync(playerQuery, cancellationToken);

        var viewModel = new PlayersIndexViewModel {
            Players = result.Items,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Query = playerQuery.Query,
            ShowAll = playerQuery.ShowAll
        };

        return View(viewModel);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayersDetailsViewModel>> DetailsAsync([FromRoute] Guid id, CancellationToken cancellationToken) {
        var result = await _playerService.GetByIdAsync(id, cancellationToken);
        if(!result.Success) {
            return NotFound();
        }

        var viewModel = new PlayersDetailsViewModel {
            Player = result.Data!,
            Notes = result.Data!.Notes,
            AwayUntil = result.Data!.AwayUntil,
            RedeemedGiftCodes = await _giftCodeService.GetRedemptionsForPlayerAsync(id, cancellationToken)
        };

        return View(viewModel);
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<PlayersDetailsViewModel>> DetailsAsync([FromRoute] Guid id, [FromForm] PlayersDetailsViewModel viewModel, CancellationToken cancellationToken) {
        var playerResult = await _playerService.GetByIdAsync(id, cancellationToken);
        if(!playerResult.Success) {
            return NotFound();
        }

        var player = playerResult.Data!;
        player.Notes = viewModel.Notes?.Trim();
        player.AwayUntil = viewModel.AwayUntil;

        var updateResult = await _playerService.UpdateAsync(player, cancellationToken);
        if(!updateResult.Success) {
            throw new Exception("Failed to update player.");
        }

        viewModel = new PlayersDetailsViewModel {
            Player = updateResult.Data!,
            Notes = updateResult.Data!.Notes,
            AwayUntil = updateResult.Data!.AwayUntil,
            RedeemedGiftCodes = await _giftCodeService.GetRedemptionsForPlayerAsync(id, cancellationToken)
        };

        return View("Details", viewModel);
    }

    [HttpGet("add")]
    public IActionResult AddPlayers() {
        var viewModel = new PlayersAddPlayersViewModel();
        return View(viewModel);
    }

    [HttpPost("add")]
    public async Task<ActionResult<PlayersAddPlayersViewModel>> AddPlayersAsync([FromForm] PlayersAddPlayersViewModel viewModel, CancellationToken cancellationToken) {
        if(!ModelState.IsValid) {
            viewModel.Results = [];
            return View(viewModel);
        }

        var playerIds = viewModel.PlayerIds?
            .Split(['\r', '\n', ','], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => {
                _ = int.TryParse(x.Trim(), out var parsedInt);
                return parsedInt;
            })
            .Where(x => x > 0)
            .OrderByDescending(x => x)
            .Distinct()
            .ToList() ?? [];

        var result = new Dictionary<string, Player?>();
        foreach(var playerId in playerIds) {
            var existingPlayer = await _playerService.GetByExternalIdAsync(playerId, cancellationToken);
            if(existingPlayer?.Data is not null) {
                result.Add(playerId.ToString(), existingPlayer.Data);
                continue;
            }

            var playerResult = await _playerService.SynchronizePlayerAsync(playerId, viewModel.AddAsInAlliance, cancellationToken);

            result.Add(playerId.ToString(), playerResult?.Data);
        }

        viewModel.Results = result;

        return View(viewModel);
    }
}
