using Microsoft.AspNetCore.Authentication;

namespace Borealis.Web.ViewModels;

public class HomeIndexViewModel {
    public bool IsAuthenticated { get; set; }
    public bool IsAllowedAccess { get; set; }
    public ICollection<AuthenticationScheme> ExternalLogins { get; set; } = [];
    public string? ReturnUrl { get; set; }
}
