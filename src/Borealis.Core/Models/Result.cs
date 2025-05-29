using System.Diagnostics;

namespace Borealis.Core.Models;

public record Result {
    public bool Success { get; }
    public ResultMessage Message { get; }

    public Result(bool success, ResultMessage message) {
        Success = success;
        Message = message;
    }
}

[DebuggerDisplay("Success: {Success} - {Message}")]
public record Result<T> : Result where T : class {
    public T? Object { get; }

    public Result(bool success, ResultMessage message, T? @object = null) : base(success, message) {
        Object = @object;
    }
}

public record PagedResult<T> : Result {
    public PagedResult() : base(true, new SuccessMessage()) { }

    public IReadOnlyCollection<T> Items { get; init; } = [];
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

[DebuggerDisplay("{Key} - {Message}")]
public abstract record ResultMessage {
    protected readonly string Format;
    protected readonly object[] Parameters;
    public string Key { get; }
    public string? SubKey { get; protected set; }
    public string Message { get; }

    public override string ToString() {
        return Message;
    }

    [DebuggerStepThrough]
    protected ResultMessage(string key, string format, params object[]? parameters) {
        Key = key;
        Format = format;
        Parameters = parameters?.Where(p => p != null).ToArray() ?? [];

        try {
            Message = string.Format(Format, Parameters);
        } catch(FormatException) {
            Message = Format;
            if(Parameters.Length > 1)
                Message += " Parameters: " + Parameters.Aggregate((s1, s2) => s1 + ", " + s2);
            else if(Parameters.Length == 1)
                Message += " Parameter: " + Parameters[0];
        }
    }
}

public record SuccessMessage : ResultMessage {
    [DebuggerStepThrough]
    public SuccessMessage() : base("Success", "Success.", null) { }
}

public record OperationFailedMessage : ResultMessage {
    [DebuggerStepThrough]
    public OperationFailedMessage(string message = "The operation failed") : base("OperationFailed", message, null) { }
}

public record GenericExceptionMessage : ResultMessage {
    [DebuggerStepThrough]
    public GenericExceptionMessage(string message) : base("GenericException", "An exception occured with the following message: {0}", message) { }
}

public record BadRequestMessage : ResultMessage {
    [DebuggerStepThrough]
    public BadRequestMessage(string message = "", string? subKey = null) : base("BadRequest", "The request content was invalid. {0}", message) {
        SubKey = subKey;
    }
}

public record NotFoundMessage : ResultMessage {
    [DebuggerStepThrough]
    public NotFoundMessage(string? id, string message = "The object with identifier {0} could not be found.") : base("NotFound", message, id ?? "<null>") { }

    [DebuggerStepThrough]
    public NotFoundMessage(Guid id, string message = "The object with identifier {0} could not be found.") : base("NotFound", message, id.ToString("D")) { }
}

public record ConflictMessage : ResultMessage {
    [DebuggerStepThrough]
    public ConflictMessage(string message = "", string? subKey = null) : base("Conflict", "There was a conflict with the request content. {0}", message) {
        SubKey = subKey;
    }
}
