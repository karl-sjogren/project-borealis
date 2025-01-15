using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core.Services;

public class UserService : QueryServiceBase<User>, IUserService {
    private readonly BorealisContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<UserService> _logger;

    protected override string DefaultSortProperty => nameof(User.Name);
    protected override bool DefaultSortAscending => false;

    public UserService(
            BorealisContext context,
            TimeProvider timeProvider,
            ILogger<UserService> logger) {
        _context = context;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<User>> GetByIdAsync(Guid userId, CancellationToken cancellationToken) {
        var entity = await _context
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if(entity is null) {
            return Results.NotFound<User>();
        }

        return Results.Success(entity);
    }

    public async Task<PagedResult<User>> GetPagedAsync(UserQuery query, CancellationToken cancellationToken) {
        var entities = await BuildQuery(_context.Users, query)
            .Skip(query.PageIndex * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await BuildQuery(_context.Users, query).CountAsync(cancellationToken);

        return Results.PagedSuccess(entities, query, totalCount);
    }

    private IQueryable<User> BuildQuery(IQueryable<User> dbQuery, UserQuery query) {
        if(!string.IsNullOrWhiteSpace(query.Query)) {
            dbQuery = dbQuery.Where(x => x.Name.Contains(query.Query));
        }

        return base.AddSorting(dbQuery, query);
    }

    public async Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken) {
        var existingPlayer = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id, cancellationToken);

        if(existingPlayer is null) {
            return Results.NotFound<User>();
        }

        existingPlayer.IsApproved = user.IsApproved;
        existingPlayer.IsAdmin = user.IsAdmin;
        existingPlayer.IsLockedOut = user.IsLockedOut;

        if(_context.Entry(existingPlayer).State == EntityState.Modified) {
            existingPlayer.UpdatedAt = _timeProvider.GetUtcNow();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(existingPlayer);
    }
}
