using Microsoft.AspNetCore.Identity;
namespace SkillSnap.Shared.Models;

public class ApplicationUser : IdentityUser
{
    public PortfolioUser? PortfolioUser { get; set; } = null!;
}