using Borealis.Core.Models;

namespace Borealis.Web.ViewModels;

public class PlayersIndexViewModel {
    public IReadOnlyCollection<WhiteoutSurvivalPlayer> Players { get; set; } = [];
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
