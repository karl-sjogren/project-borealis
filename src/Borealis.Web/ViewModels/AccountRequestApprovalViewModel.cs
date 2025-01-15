using System.ComponentModel.DataAnnotations;

namespace Borealis.Web.ViewModels;

public class AccountRequestApprovalViewModel {
    [Required]
    public string? Name { get; set; }

    public bool IsInitialUser { get; set; }
    public bool IsPendingApproval { get; set; }
}
