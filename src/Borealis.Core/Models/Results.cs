using Borealis.Core.Requests;

namespace Borealis.Core.Models;

public static class Results {
    public static Result Success() => new(true, new SuccessMessage());

    public static Result<T> Success<T>(T? @object = null) where T : class => new(true, new SuccessMessage(), @object);

    public static Result Success(ResultMessage resultMessage) => new(true, resultMessage);

    public static Result<T> Success<T>(ResultMessage resultMessage, T? @object = null) where T : class => new(true, resultMessage, @object);

    public static Result Failure(ResultMessage resultMessage) => new(false, resultMessage);

    public static Result<T> Failure<T>(ResultMessage resultMessage, T? @object = null) where T : class => new(false, resultMessage, @object);

    public static Result NotFound() => new(false, new NotFoundMessage(null));

    public static Result NotFound(string id) => new(false, new NotFoundMessage(id));

    public static Result NotFound(Guid id) => new(false, new NotFoundMessage(id));

    public static Result<T> NotFound<T>() where T : class => new(false, new NotFoundMessage(null));

    public static Result<T> NotFound<T>(string id) where T : class => new(false, new NotFoundMessage(id));

    public static Result<T> NotFound<T>(Guid id) where T : class => new(false, new NotFoundMessage(id));

    public static PagedResult<T> PagedSuccess<T>(IReadOnlyCollection<T> items, QueryBase query, int totalCount) => new() { Items = items, PageIndex = query.PageIndex, PageSize = query.PageSize, TotalCount = totalCount };
}
