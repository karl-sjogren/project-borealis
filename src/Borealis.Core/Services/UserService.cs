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
        var entities = await BuildQuery(_context.Users.AsNoTracking(), query)
            .Skip(query.PageIndex * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await BuildQuery(_context.Users, query).CountAsync(cancellationToken);

        return Results.PagedSuccess(entities, query, totalCount);
    }

    private IQueryable<User> BuildQuery(IQueryable<User> dbQuery, UserQuery query) {
        if(!string.IsNullOrWhiteSpace(query.Query)) {
            dbQuery = dbQuery.Where(x => EF.Functions.Like(x.Name, $"%{query.Query}%"));
        }

        return base.AddSorting(dbQuery, query);
    }

    public async Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken) {
        var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id, cancellationToken);

        if(existingUser is null) {
            return Results.NotFound<User>();
        }

        existingUser.IsApproved = user.IsApproved;
        existingUser.IsAdmin = user.IsAdmin;
        existingUser.IsLockedOut = user.IsLockedOut;

        if(_context.Entry(existingUser).State == EntityState.Modified) {
            existingUser.UpdatedAt = _timeProvider.GetUtcNow();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(existingUser);
    }

    public async Task<Result> DeleteAsync(Guid userId, CancellationToken cancellationToken) {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if(user is null) {
            return Results.NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success();
    }
}
