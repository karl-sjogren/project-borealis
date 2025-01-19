namespace Borealis.Core.Models;

public class MessageTemplate : EntityBase {
    public required string Name { get; set; }
    public required string Message { get; set; }
    public IList<MessageTemplateHistoryEntry> HistorialMessages { get; set; } = [];
}

public class MessageTemplateHistoryEntry {
    public required string Message { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
}
