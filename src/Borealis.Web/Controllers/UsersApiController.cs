using Borealis.Core.Contracts;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("api/users")]
[Authorize(Roles = "AdminUser")]
public class UsersApiController : Controller {
    private readonly IUserService _userService;
    private readonly ILogger<UsersApiController> _logger;

    public UsersApiController(IUserService userService, ILogger<UsersApiController> logger) {
        _userService = userService;
        _logger = logger;
    }

    [HttpDelete("{userId:guid}")]
    public async Task<ActionResult<PlayersIndexViewModel>> DeleteAsync(Guid userId, CancellationToken cancellationToken) {
        var result = await _userService.DeleteAsync(userId, cancellationToken);

        if(!result.Success) {
            return NotFound();
        }

        return NoContent();
    }
}
