using System.Diagnostics;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller {
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<HomeController> _logger;

    public HomeController(SignInManager<IdentityUser> signInManager, ILogger<HomeController> logger) {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> IndexAsync() {
        var viewModel = new HomeIndexViewModel {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            IsAllowedAccess = User.IsInRole("TrustedUser"),
            ExternalLogins = [.. await _signInManager.GetExternalAuthenticationSchemesAsync()],
            ReturnUrl = Url.Content("~/")
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
