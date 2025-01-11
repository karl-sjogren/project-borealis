namespace Borealis.Core.Requests;

public class QueryBase {
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 25;
    public string? SortField { get; set; }
    public bool SortAscending { get; set; }
}
