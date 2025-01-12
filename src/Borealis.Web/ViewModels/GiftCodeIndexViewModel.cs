using Borealis.Core.Models;

namespace Borealis.Web.ViewModels;

public class GiftCodeIndexViewModel {
    public IReadOnlyCollection<GiftCode> GiftCodes { get; set; } = [];
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string Query { get; set; } = "";
    public bool ShowAll { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => PageIndex < TotalPages - 1;
}
