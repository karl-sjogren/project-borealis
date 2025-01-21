using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Borealis.Core;
using Borealis.Core.Models;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Web.Controllers;

[Authorize]
public class AccountController : Controller {
    private readonly BorealisContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AccountController> _logger;

    public AccountController(BorealisContext context, TimeProvider timeProvider, ILogger<AccountController> logger) {
        _context = context;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    [HttpGet("request-approval")]
    public async Task<IActionResult> RequestApprovalAsync() {
        var viewModel = new AccountRequestApprovalViewModel {
            IsInitialUser = !await _context.Users.AnyAsync(),
            IsPendingApproval = User.IsInRole("PendingApproval")
        };

        return View(viewModel);
    }

    [HttpPost("request-approval")]
    public async Task<IActionResult> RequestApprovalAsync([FromForm] AccountRequestApprovalViewModel viewModel) {
        if(!ModelState.IsValid) {
            return View(viewModel);
        }

        var externalIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if(externalIdClaim is null) {
            return RedirectToAction("Index");
        }

        var isInitialUser = !await _context.Users.AnyAsync();

        var now = _timeProvider.GetUtcNow();
        var user = new User {
            Name = viewModel.Name!,
            ExternalId = externalIdClaim.Value,
            IsAdmin = isInitialUser,
            IsApproved = isInitialUser,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var claims = new List<Claim> {
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.NameIdentifier, user.ExternalId)
        };

        if(!isInitialUser) {
            claims.Add(new(ClaimTypes.Role, "PendingApproval"));
        } else {
            claims.Add(new(ClaimTypes.Role, "TrustedUser"));
            claims.Add(new(ClaimTypes.Role, "AdminUser"));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if(isInitialUser) {
            return RedirectToAction("Index", "Home");
        } else {
            return RedirectToAction("RequestApproval");
        }
    }

    [HttpGet("discord-login")]
    [AllowAnonymous]
    public IActionResult DiscordLogin() {
        var properties = new AuthenticationProperties {
            RedirectUri = Url.Action("Index", "Home")
        };

        return Challenge(properties, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("relogin")]
    public async Task<IActionResult> ReloginAsync() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var properties = new AuthenticationProperties {
            RedirectUri = Url.Action("Index", "Home")
        };

        return Challenge(properties, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("sign-out")]
    public async Task<IActionResult> SignOutAsync(string? returnUrl) {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if(!string.IsNullOrWhiteSpace(returnUrl)) {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
