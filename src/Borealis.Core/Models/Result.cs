using Borealis.Core.Requests;

namespace Borealis.Core.Models;

public record Result {
    public required bool Success { get; init; }
    public string Message { get; init; } = "Unknown result";
}

public record Result<T> : Result {
    public T? Data { get; init; }
}

public record PagedResult<T> : Result {
    public IReadOnlyCollection<T> Items { get; init; } = [];
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public static class Results {
    public static Result Success() => new() { Success = true, Message = "Success" };
    public static Result<T> Success<T>(T data) => new() { Success = true, Message = "Success", Data = data };
    public static Result Failure(string message) => new() { Success = false, Message = message };
    public static Result<T> Failure<T>(string message) => new() { Success = false, Message = message };

    public static PagedResult<T> PagedSuccess<T>(IReadOnlyCollection<T> items, QueryBase query, int totalCount) => new() { Success = true, Message = "Success", Items = items, PageIndex = query.PageIndex, PageSize = query.PageSize, TotalCount = totalCount };
    public static Result<T> NotFound<T>() => new() { Success = false, Message = "Not found" };
}
