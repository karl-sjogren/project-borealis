using Borealis.Core.Models;
using Borealis.Core.Requests;

namespace Borealis.Core.Contracts;

public interface IUserService {
    Task<Result<User>> GetByIdAsync(Guid playuserIdrId, CancellationToken cancellationToken);
    Task<PagedResult<User>> GetPagedAsync(UserQuery query, CancellationToken cancellationToken);
    Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken);
}
