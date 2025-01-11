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
    public static PagedResult<T> PagedSuccess<T>(IReadOnlyCollection<T> items, int pageIndex, int pageSize, int totalCount) => new() { Success = true, Message = "Success", Items = items, PageIndex = pageIndex, PageSize = pageSize, TotalCount = totalCount };
    public static Result<T> NotFound<T>() => new() { Success = false, Message = "Not found" };
}
