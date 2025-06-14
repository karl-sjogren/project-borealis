using Borealis.Core.Contracts;
using Borealis.Core.Requests;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("users")]
[Authorize(Roles = "AdminUser")]
public class UsersController : Controller {
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger) {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<UsersIndexViewModel>> IndexAsync([FromQuery] UserQuery userQuery, CancellationToken cancellationToken) {
        var result = await _userService.GetPagedAsync(userQuery, cancellationToken);

        var viewModel = new UsersIndexViewModel {
            Users = result.Items,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Query = userQuery.Query
        };

        return View(viewModel);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsersDetailsViewModel>> DetailsAsync([FromRoute] Guid id, CancellationToken cancellationToken) {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        if(!result.Success) {
            return NotFound();
        }

        var user = result.Object!;
        var viewModel = new UsersDetailsViewModel {
            User = user,
            IsAdmin = user.IsAdmin,
            IsApproved = user.IsApproved,
            IsLockedOut = user.IsLockedOut
        };

        return View(viewModel);
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<UsersDetailsViewModel>> DetailsAsync([FromRoute] Guid id, [FromForm] UsersDetailsViewModel viewModel, CancellationToken cancellationToken) {
        var userResult = await _userService.GetByIdAsync(id, cancellationToken);
        if(!userResult.Success) {
            return NotFound();
        }

        var user = userResult.Object!;
        user.IsAdmin = viewModel.IsAdmin;
        user.IsApproved = viewModel.IsApproved;
        user.IsLockedOut = viewModel.IsLockedOut;

        var updateResult = await _userService.UpdateAsync(user, cancellationToken);
        if(!updateResult.Success) {
            throw new Exception("Failed to set player notes.");
        }

        user = updateResult.Object!;
        viewModel = new UsersDetailsViewModel {
            User = user,
            IsAdmin = user.IsAdmin,
            IsApproved = user.IsApproved,
            IsLockedOut = user.IsLockedOut
        };

        return View("Details", viewModel);
    }
}
