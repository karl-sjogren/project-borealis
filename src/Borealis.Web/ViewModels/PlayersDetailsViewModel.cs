using Borealis.Core.Models;

namespace Borealis.Web.ViewModels;

public class PlayersDetailsViewModel {
    public required Player Player { get; set; }
    public string? Notes { get; set; }
}
