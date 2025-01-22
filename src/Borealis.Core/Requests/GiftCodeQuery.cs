namespace Borealis.Core.Requests;

public class GiftCodeQuery : QueryBase {
    public string Query { get; set; } = "";
    public bool? IsExpired { get; set; }
}
