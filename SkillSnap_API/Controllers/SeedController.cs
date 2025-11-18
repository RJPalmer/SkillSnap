using Microsoft.AspNetCore.Mvc;
using SkillSnap.Shared.Models;
using SkillSnap_API.Data;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : Controller
    {
        private readonly SkillSnapDbContext _context;

        // Constructor
        public SeedController(SkillSnapDbContext context)
        {
            _context = context;
        }
        // GET: SeedController
        public IActionResult Index()
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult Seed()
        {
            if (_context.PortfolioUsers.Any())
            {
                return Conflict("Sample data already exists.");
            }

            // Create a sample user
            var user = new PortfolioUser
            {
                Name = "Jordan Developer",
                Bio = "Full-stack developer passionate about learning new tech.",
                ProfileImageUrl = "https://example.com/images/jordan.png",
                portfolioUserProjects = new List<PortfolioUserProject>()
            };
            var projects = new List<Project>
                {
                    new Project { Title = "Task Tracker", Description = "Manage tasks effectively", ImageUrl = "https://example.com/images/task.png" },
                    new Project { Title = "Weather App", Description = "Forecast weather using APIs", ImageUrl = "https://example.com/images/weather.png" }
            };

            // Create sample skills
            var skills = new List<Skill>
            {
                new Skill { Name = "C#", Level = "Advanced" },
                 new Skill { Name = "Blazor", Level = "Intermediate" }
            };

            _context.PortfolioUsers.Add(user);
            _context.Projects.AddRange(projects);
            _context.Skills.AddRange(skills);
            _context.SaveChanges();

            //create join entries between user and projects
            var userProjects = projects.Select(project => new PortfolioUserProject{
                PortfolioUser = user,
                PortfolioUserId = user.Id,
                Project = project,
                ProjectId = project.Id
            }).ToList();


            // Create join entries between user and skills
            var userSkills = skills.Select(skill => new PortfolioUserSkill
            {
                PortfolioUser = user,
                PortfolioUserId = user.Id,
                Skill = skill,
                SkillId = skill.Id
            }).ToList();

            _context.PortfolioUserSkills.AddRange(userSkills);
            _context.PortfolioUserProjects.AddRange(userProjects);
            _context.SaveChanges();

            return Ok("Sample data inserted successfully using join table relationships.");
        }
    }
}
