using Borealis.Core.Contracts;
using Borealis.Core.Requests;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("gift-codes")]
[Authorize(Roles = "TrustedUser")]
public class GiftCodeController : Controller {
    private readonly IGiftCodeService _giftCodeService;
    private readonly IGiftCodeRedemptionQueue _giftCodeRedemptionQueue;
    private readonly IPlayerService _playerService;
    private readonly ILogger<GiftCodeController> _logger;

    public GiftCodeController(
            IGiftCodeService giftCodeService,
            IGiftCodeRedemptionQueue giftCodeRedemptionQueue,
            IPlayerService playerService,
            ILogger<GiftCodeController> logger) {
        _giftCodeService = giftCodeService;
        _giftCodeRedemptionQueue = giftCodeRedemptionQueue;
        _playerService = playerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GiftCodeIndexViewModel>> IndexAsync([FromQuery] GiftCodeQuery giftCodeQuery, CancellationToken cancellationToken) {
        var result = await _giftCodeService.GetPagedAsync(giftCodeQuery, cancellationToken);
        var queueLength = await _giftCodeRedemptionQueue.GetQueueLengthAsync(cancellationToken);

        var viewModel = new GiftCodeIndexViewModel {
            GiftCodes = result.Items,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Query = giftCodeQuery.Query,
            CurrentlyRedeeming = queueLength
        };

        return View(viewModel);
    }

    [HttpGet("add")]
    public IActionResult AddGiftCode() {
        var viewModel = new GiftCodeAddGiftCodeViewModel();
        return View(viewModel);
    }

    [HttpPost("add")]
    public async Task<ActionResult<GiftCodeAddGiftCodeViewModel>> AddGiftCodeAsync([FromForm] GiftCodeAddGiftCodeViewModel viewModel, CancellationToken cancellationToken) {
        if(!ModelState.IsValid) {
            viewModel.ResultString = null;
            return View(viewModel);
        }

        if(string.IsNullOrWhiteSpace(viewModel.Code)) {
            viewModel.ResultString = "Code is required.";
            return View(viewModel);
        }

        var cleanedCode = viewModel.Code.Trim();
        var result = await _giftCodeService.AddGiftCodeAsync(cleanedCode, cancellationToken);

        if(result.Success) {
            viewModel.Success = true;
            viewModel.ResultString = "Gift code added. Started redemption.";
        } else {
            viewModel.Success = false;
            viewModel.ResultString = result.Message;
        }

        return View(viewModel);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GiftCodeDetailsViewModel>> DetailsAsync([FromRoute] Guid id, CancellationToken cancellationToken) {
        var result = await _giftCodeService.GetByIdAsync(id, cancellationToken);
        if(!result.Success) {
            return NotFound();
        }

        var redemptions = await _giftCodeService.GetRedemptionsForGiftCodeAsync(id, cancellationToken);
        var players = await _playerService.GetPagedAsync(new PlayerQuery { PageIndex = 0, PageSize = 10_000, ShowAll = true }, cancellationToken);

        var playersNotRedeemedFor = players.Items.Where(x => x.IsInAlliance && !redemptions.Any(y => y.PlayerId == x.Id)).ToList();

        var viewModel = new GiftCodeDetailsViewModel {
            Code = result.Data!,
            Redemptions = redemptions,
            PlayersNotRedeemedFor = playersNotRedeemedFor
        };

        return View(viewModel);
    }

    [HttpGet("queue")]
    public async Task<ActionResult<GiftCodeQueueViewModel>> ShowQueueAsync(CancellationToken cancellationToken) {
        var items = await _giftCodeRedemptionQueue.GetQueueAsync(cancellationToken);

        var viewModel = new GiftCodeQueueViewModel {
            Items = items
        };

        return View(viewModel);
    }
}
