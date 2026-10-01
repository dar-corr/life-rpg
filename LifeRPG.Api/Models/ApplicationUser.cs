using Microsoft.AspNetCore.Identity;

namespace LifeRPG.Api.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}