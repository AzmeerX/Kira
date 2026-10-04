using Microsoft.AspNetCore.Identity;

namespace Kira.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = "Developer";
}