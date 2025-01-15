using System.Diagnostics;
using System.Security.Claims;
using Borealis.Core;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Web.Controllers;

public class HomeController : Controller {
    private readonly BorealisContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(BorealisContext context, ILogger<HomeController> logger) {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> IndexAsync() {
        var externalIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if(externalIdClaim is null) {
            return View(new HomeIndexViewModel {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.ExternalId == externalIdClaim.Value);

        var viewModel = new HomeIndexViewModel {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            IsPendingApproval = User.IsInRole("PendingApproval"),
            IsApprovedUser = User.IsInRole("TrustedUser")
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
