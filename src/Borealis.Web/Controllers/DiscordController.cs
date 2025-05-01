using Borealis.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("discord")]
[Authorize(Roles = "AdminUser")]
public class DiscordController : Controller {
    private readonly IDiscordBotService _discordBotService;
    private readonly ILogger<DiscordController> _logger;

    public DiscordController(
            IDiscordBotService discordBotService,
            ILogger<DiscordController> logger) {
        _discordBotService = discordBotService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult Index() {
        return View();
    }
}
