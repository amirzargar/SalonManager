using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? FullName { get; set; }
}
