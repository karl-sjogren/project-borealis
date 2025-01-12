using Borealis.Core.Requests;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Web.Controllers;

[Route("users")]

public class UsersController : Controller {
    private readonly UserManager<IdentityUser> _userManager;

    public UsersController(UserManager<IdentityUser> userManager) {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> IndexAsync([FromQuery] UserQuery userQuery, CancellationToken cancellationToken) {
        var users = await _userManager
            .Users
            .Skip(userQuery.PageIndex * userQuery.PageSize)
            .Take(userQuery.PageSize)
            .ToListAsync(cancellationToken);

        var viewModel = new UsersIndexViewModel {
            Users = users,
            PageIndex = userQuery.PageIndex,
            PageSize = userQuery.PageSize,
            TotalCount = await _userManager.Users.CountAsync(cancellationToken),
        };

        return View(viewModel);
    }
}
