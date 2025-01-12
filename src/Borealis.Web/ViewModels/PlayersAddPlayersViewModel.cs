using Borealis.Core.Models;

namespace Borealis.Web.ViewModels;

public class PlayersAddPlayersViewModel {
    public string? PlayerIds { get; set; }
    public ICollection<KeyValuePair<string, Player?>> Results { get; set; } = [];
}
