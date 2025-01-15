namespace Borealis.Core.Models;

public class User : EntityBase {
    public required string Name { get; set; }
    public required string ExternalId { get; set; }
    public bool IsApproved { get; set; }
    public bool IsLockedOut { get; set; }
    public bool IsAdmin { get; set; }
}
