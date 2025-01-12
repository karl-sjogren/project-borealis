using Borealis.Core.Contracts;
using Borealis.Core.Requests;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("gift-codes")]
public class GiftCodeController : Controller {
    private readonly IGiftCodeService _giftCodeService;
    private readonly ILogger<GiftCodeController> _logger;

    public GiftCodeController(IGiftCodeService giftCodeService, ILogger<GiftCodeController> logger) {
        _giftCodeService = giftCodeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PlayersIndexViewModel>> IndexAsync([FromQuery] GiftCodeQuery giftCodeQuery, CancellationToken cancellationToken) {
        var result = await _giftCodeService.GetPagedAsync(giftCodeQuery, cancellationToken);

        var viewModel = new GiftCodeIndexViewModel {
            GiftCodes = result.Items,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Query = giftCodeQuery.Query
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
}
