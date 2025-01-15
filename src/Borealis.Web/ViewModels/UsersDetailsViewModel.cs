using Borealis.Core.Models;

namespace Borealis.Web.ViewModels;

public class UsersDetailsViewModel {
    public required User User { get; set; }
    public bool IsApproved { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsLockedOut { get; set; }
}
