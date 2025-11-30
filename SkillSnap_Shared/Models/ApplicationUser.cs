using Microsoft.AspNetCore.Identity;
namespace SkillSnap.Shared.Models;

public class ApplicationUser : IdentityUser
{
    // One-to-one link to PortfolioUser (nullable until created)
    public PortfolioUser? PortfolioUser { get; set; } = null!;

    // Navigation property for Identity Roles
    public ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();

    // Helper: check if user has a role
    public bool HasRole(string roleId)
    {
        return UserRoles.Any(ur => ur.RoleId == roleId);
    }

    // Helper: add a role to the user (id only; persistence handled by UserManager)
    public void AddRole(string roleId)
    {
        if (!HasRole(roleId))
        {
            UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = this.Id,
                RoleId = roleId
            });
        }
    }

    // Helper: remove a role from the user
    public void RemoveRole(string roleId)
    {
        var existing = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (existing != null)
        {
            UserRoles.Remove(existing);
        }
    }
}