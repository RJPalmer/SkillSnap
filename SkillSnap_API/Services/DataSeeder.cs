using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap_API.Data;

namespace SkillSnap_API.Services;

/// <summary>
/// Seeds initial application data: roles, admin user, and sample PortfolioUser entries.
/// Designed to run automatically at app startup.
/// </summary>
public class DataSeeder
{
    /// <summary>
    /// Generates a consistent random placeholder image URL.
    /// </summary>
    private string GenerateRandomImageUrl(int size = 200)
    {
        return $"https://picsum.photos/seed/{Guid.NewGuid()}/{size}";
    }

    private readonly SkillSnapDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public DataSeeder(
        SkillSnapDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // ---- Seed Roles ----
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ---- Seed Admin Account ----
            var adminEmail = _configuration["AdminUser:Email"] ?? "admin@skillsnap.com";
            var adminPassword = _configuration["AdminUser:Password"] ?? "Admin123!";

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // ---- Link Admin to a PortfolioUser (if needed) ----
            if (!await _context.PortfolioUsers.AnyAsync(pu => pu.ApplicationUserId == adminUser.Id))
            {
                _context.PortfolioUsers.Add(new PortfolioUser
                {
                    Name = "Admin",
                    Bio = "Administrator account",
                    ProfileImageUrl = GenerateRandomImageUrl(),
                    ApplicationUserId = adminUser.Id
                });
                await _context.SaveChangesAsync();
            }

            // ---- Seed Sample PortfolioUser Data ----
            if (!await _context.PortfolioUsers.AnyAsync(pu => pu.ApplicationUserId != adminUser.Id))
            {
                var portfolioUser = new PortfolioUser
                {
                    Name = "Jordan Developer",
                    Bio = "Full-stack developer passionate about learning new tech.",
                    ProfileImageUrl = GenerateRandomImageUrl()
                };

                _context.PortfolioUsers.Add(portfolioUser);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}