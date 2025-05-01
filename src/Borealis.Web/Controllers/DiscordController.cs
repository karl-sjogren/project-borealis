using Borealis.Core;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Web.Controllers;

[Route("discord")]
[Authorize(Roles = "AdminUser")]
public class DiscordController : Controller {
    private readonly BorealisContext _borealisContext;
    private readonly ILogger<DiscordController> _logger;

    public DiscordController(
            BorealisContext borealisContext,
            ILogger<DiscordController> logger) {
        _borealisContext = borealisContext;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult Index() {
        var settings = _borealisContext.DiscordNotificationSettings
            .AsNoTracking()
            .FirstOrDefault();

        var viewModel = new DiscordIndexViewModel {
            Settings = settings
        };

        return View(viewModel);
    }
}
