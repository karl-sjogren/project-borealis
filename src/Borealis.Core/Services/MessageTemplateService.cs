using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core.Services;

public class MessageTemplateService : QueryServiceBase<MessageTemplate>, IMessageTemplateService {
    private readonly BorealisContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MessageTemplateService> _logger;

    protected override string DefaultSortProperty => nameof(MessageTemplate.UpdatedAt);
    protected override bool DefaultSortAscending => true;

    public MessageTemplateService(
            BorealisContext context,
            TimeProvider timeProvider,
            ILogger<MessageTemplateService> logger) {
        _context = context;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<MessageTemplate>> GetByIdAsync(Guid messageTemplateId, CancellationToken cancellationToken) {
        var entity = await _context
            .MessageTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == messageTemplateId, cancellationToken);

        if(entity is null) {
            return Results.NotFound<MessageTemplate>();
        }

        return Results.Success(entity);
    }

    public async Task<PagedResult<MessageTemplate>> GetPagedAsync(MessageTemplateQuery query, CancellationToken cancellationToken) {
        var entities = await BuildQuery(_context.MessageTemplates.AsNoTracking(), query)
            .Skip(query.PageIndex * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await BuildQuery(_context.MessageTemplates, query).CountAsync(cancellationToken);

        return Results.PagedSuccess(entities, query, totalCount);
    }

    private IQueryable<MessageTemplate> BuildQuery(IQueryable<MessageTemplate> dbQuery, MessageTemplateQuery query) {
        if(!string.IsNullOrWhiteSpace(query.Query)) {
            dbQuery = dbQuery.Where(x => x.Name.Contains(query.Query) || x.Message.Contains(query.Query));
        }

        return base.AddSorting(dbQuery, query);
    }

    public async Task<Result<MessageTemplate>> CreateAsync(string name, string message, CancellationToken cancellationToken) {
        var now = _timeProvider.GetUtcNow();

        var newTemplate = new MessageTemplate {
            Name = name,
            Message = message,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.MessageTemplates.AddAsync(newTemplate, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(newTemplate);
    }

    public async Task<Result<MessageTemplate>> UpdateAsync(MessageTemplate messageTemplate, CancellationToken cancellationToken) {
        var existingTemplate = await _context.MessageTemplates.FirstOrDefaultAsync(x => x.Id == messageTemplate.Id, cancellationToken);

        if(existingTemplate is null) {
            return Results.NotFound<MessageTemplate>();
        }

        var historyEntry = new MessageTemplateHistoryEntry {
            Message = existingTemplate.Message,
            Timestamp = existingTemplate.UpdatedAt
        };

        existingTemplate.Name = messageTemplate.Name;
        existingTemplate.Message = messageTemplate.Message;
        existingTemplate.UpdatedAt = _timeProvider.GetUtcNow();
        existingTemplate.HistorialMessages.Add(historyEntry);

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(existingTemplate);
    }
}
