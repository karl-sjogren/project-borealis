using Borealis.Core.Contracts;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

public class PlayersController : Controller {
    private readonly IPlayerService _playerService;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(IPlayerService playerService, ILogger<PlayersController> logger) {
        _playerService = playerService;
        _logger = logger;
    }

    public async Task<ActionResult<PlayersIndexViewModel>> IndexAsync([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default) {
        var result = await _playerService.GetPagedAsync(pageIndex, pageSize, cancellationToken);

        var viewModel = new PlayersIndexViewModel {
            Players = result.Items,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };

        return View(viewModel);
    }
}
