namespace Borealis.Core.Requests;

public class PlayerQuery : QueryBase {
    public string Query { get; set; } = "";
    public bool ShowAll { get; set; }
}
