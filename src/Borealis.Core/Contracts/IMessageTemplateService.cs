using Borealis.Core.Models;
using Borealis.Core.Requests;

namespace Borealis.Core.Contracts;

public interface IMessageTemplateService {
    Task<Result<MessageTemplate>> GetByIdAsync(Guid messageTemplateId, CancellationToken cancellationToken);
    Task<PagedResult<MessageTemplate>> GetPagedAsync(MessageTemplateQuery query, CancellationToken cancellationToken);
    Task<Result<MessageTemplate>> CreateAsync(string name, string message, CancellationToken cancellationToken);
    Task<Result<MessageTemplate>> UpdateAsync(MessageTemplate messageTemplate, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid messageTemplateId, CancellationToken cancellationToken);
}
