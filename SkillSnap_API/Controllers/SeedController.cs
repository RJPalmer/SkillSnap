using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SkillSnap.Shared.Models;
using SkillSnap_API.Data;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly SkillSnapDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public SeedController(
            SkillSnapDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Seed Admin/User roles + sample data + optional admin account
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Seed()
        {
            // ------------------------------------------------------------
            // 1. Seed Identity Roles (Admin, User)
            // ------------------------------------------------------------
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ------------------------------------------------------------
            // 2. Seed default Admin account (optional)
            // ------------------------------------------------------------
            var adminEmail = "admin@skillsnap.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(adminUser, "Admin123!");
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // ------------------------------------------------------------
            // 3. Prevent duplicate seeding
            // ------------------------------------------------------------
            if (_context.PortfolioUsers.Any())
                return Conflict("Sample data already exists.");

            // ------------------------------------------------------------
            // 4. Create PortfolioUser sample data
            // ------------------------------------------------------------
            var portfolioUser = new PortfolioUser
            {
                Name = "Jordan Developer",
                Bio = "Full-stack developer passionate about learning new tech.",
                ProfileImageUrl = "https://example.com/images/jordan.png",
                PortfolioUserProjects = new List<PortfolioUserProject>(),
                PortfolioUserSkills = new List<PortfolioUserSkill>()
            };

            var projects = new List<Project>
            {
                new Project
                {
                    Title = "Task Tracker",
                    Description = "Manage tasks effectively",
                    ImageUrl = "https://example.com/images/task.png"
                },
                new Project
                {
                    Title = "Weather App",
                    Description = "Forecast weather using APIs",
                    ImageUrl = "https://example.com/images/weather.png"
                }
            };

            var skills = new List<Skill>
            {
                new Skill { Name = "C#", Level = "Advanced" },
                new Skill { Name = "Blazor", Level = "Intermediate" }
            };

            // Add entities to DB
            _context.PortfolioUsers.Add(portfolioUser);
            _context.Projects.AddRange(projects);
            _context.Skills.AddRange(skills);
            await _context.SaveChangesAsync();

            // ------------------------------------------------------------
            // 5. Create Join Table Entries
            // ------------------------------------------------------------
            var userProjects = projects.Select(project => new PortfolioUserProject
            {
                PortfolioUserId = portfolioUser.Id,
                ProjectId = project.Id
            }).ToList();

            var userSkills = skills.Select(skill => new PortfolioUserSkill
            {
                PortfolioUserId = portfolioUser.Id,
                SkillId = skill.Id,
                Skill = skill,
                PortfolioUser = portfolioUser
            }).ToList();

            _context.PortfolioUserProjects.AddRange(userProjects);
            _context.PortfolioUserSkills.AddRange(userSkills);

            await _context.SaveChangesAsync();

            return Ok("Roles + Admin account + Sample data seeded successfully.");
        }
    }
}