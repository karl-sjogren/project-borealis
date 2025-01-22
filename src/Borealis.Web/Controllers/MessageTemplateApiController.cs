using Borealis.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Borealis.Web.Controllers;

[Route("api/messages")]
[Authorize(Roles = "TrustedUser")]
public class MessageTemplateApiController : Controller {
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly ILogger<MessageTemplateApiController> _logger;

    public MessageTemplateApiController(IMessageTemplateService messageTemplateService, ILogger<MessageTemplateApiController> logger) {
        _messageTemplateService = messageTemplateService;
        _logger = logger;
    }

    [HttpDelete("{messageTemplateId:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid messageTemplateId, CancellationToken cancellationToken) {
        var result = await _messageTemplateService.DeleteAsync(messageTemplateId, cancellationToken);

        if(!result.Success) {
            return NotFound();
        }

        return NoContent();
    }
}
