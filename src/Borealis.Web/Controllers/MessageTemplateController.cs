using Borealis.Core.Contracts;
using Borealis.Core.Requests;
using Borealis.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("messages")]
[Authorize(Roles = "TrustedUser")]
public class MessageTemplateController : Controller {
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly ILogger<MessageTemplateController> _logger;

    public MessageTemplateController(IMessageTemplateService messageTemplateService, ILogger<MessageTemplateController> logger) {
        _messageTemplateService = messageTemplateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<MessageTemplateIndexViewModel>> IndexAsync([FromQuery] MessageTemplateQuery messageTemplateQuery, CancellationToken cancellationToken) {
        var result = await _messageTemplateService.GetPagedAsync(messageTemplateQuery, cancellationToken);

        var viewModel = new MessageTemplateIndexViewModel {
            MessageTemplates = result.Items,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Query = messageTemplateQuery.Query
        };

        return View(viewModel);
    }

    [HttpGet("new")]
    public ActionResult<MessageTemplateEditViewModel> New() {
        return View();
    }

    [HttpPost("new")]
    public async Task<ActionResult> NewAsync([FromForm] string name, string message, CancellationToken cancellationToken) {
        var createResult = await _messageTemplateService.CreateAsync(name, message, cancellationToken);
        if(!createResult.Success) {
            throw new Exception("Failed to update message template.");
        }

        var viewModel = new MessageTemplateEditViewModel {
            MessageTemplate = createResult.Data!
        };

        return RedirectToAction("Edit", new { id = viewModel.MessageTemplate.Id });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MessageTemplateEditViewModel>> EditAsync([FromRoute] Guid id, CancellationToken cancellationToken) {
        var result = await _messageTemplateService.GetByIdAsync(id, cancellationToken);
        if(!result.Success) {
            return NotFound();
        }

        var viewModel = new MessageTemplateEditViewModel {
            MessageTemplate = result.Data!
        };

        return View(viewModel);
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<MessageTemplateEditViewModel>> EditAsync([FromRoute] Guid id, [FromForm] string name, [FromForm] string message, CancellationToken cancellationToken) {
        var messageTemplateResult = await _messageTemplateService.GetByIdAsync(id, cancellationToken);
        if(!messageTemplateResult.Success) {
            return NotFound();
        }

        var messageTemplate = messageTemplateResult.Data!;
        messageTemplate.Name = name;
        messageTemplate.Message = message;

        var updateResult = await _messageTemplateService.UpdateAsync(messageTemplate, cancellationToken);
        if(!updateResult.Success) {
            throw new Exception("Failed to update message template.");
        }

        var viewModel = new MessageTemplateEditViewModel {
            MessageTemplate = updateResult.Data!
        };

        return View("Edit", viewModel);
    }
}
