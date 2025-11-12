using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using SkillSnap_API.Data;
using System.Net;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioUserController : ControllerBase
    {
        private readonly SkillSnapDbContext _context;

        public PortfolioUserController(SkillSnapDbContext context)
        {
            _context = context;
        }

        // GET: api/PortfolioUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioUserDto>>> GetAll()
        {
            var users = await _context.PortfolioUsers
                .Include(p => p.Projects)
                .Include(p => p.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .ToListAsync();

            var dtos = users.Select(u => new PortfolioUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                Projects = u.Projects.Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserId = p.PortfolioUserId
                }).ToList(),
                portfolioUserSkills = u.PortfolioUserSkills.Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    PortfolioUsers = s.PortfolioUsers,
                    // PortfolioUserId = s.PortfolioUserId
                }).ToList()
            });

            return Ok(dtos);
        }

        // GET: api/PortfolioUser/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PortfolioUserDto>> GetById(int id)
        {
            var user = await _context.PortfolioUsers
                .Include(p => p.Projects)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var dto = new PortfolioUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                Projects = user.Projects.Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserId = p.PortfolioUserId
                }).ToList(),
                Skills = user.Skills.Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    // PortfolioUserId = s.PortfolioUserId
                    PortfolioUsers = s.PortfolioUsers
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/PortfolioUser/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<PortfolioUserDto>> GetByName(string name)
        {
            var user = await _context.PortfolioUsers
                .Include(p => p.Projects)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(u => u.Name == name);

            if (user == null)
                return NotFound();

            var dto = new PortfolioUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                Projects = user.Projects.Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserId = p.PortfolioUserId
                }).ToList(),
                Skills = user.Skills.Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    PortfolioUsers = s.PortfolioUsers
                    // PortfolioUserId = s.PortfolioUserId
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/PortfolioUser/1/projects
        [HttpGet("{id}/projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects(int id)
        {
            var user = await _context.PortfolioUsers
                .Include(p => p.Projects)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var projectDtos = user.Projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                PortfolioUserId = p.PortfolioUserId
            });

            return Ok(projectDtos);
        }
        // GET: api/PortfolioUser/1/skills
        [HttpGet("{id}/skills")]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetUserSkills(int id)
        {
            var user = await _context.PortfolioUsers
                .Include(u => u.Skills)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var skillDtos = user.Skills.Select(s => new SkillDto
            {
                Id = s.Id,
                Name = s.Name,
                Level = s.Level,
                PortfolioUsers = s.PortfolioUsers
                // PortfolioUserId = s.PortfolioUserId
            });

            return Ok(skillDtos);
        }

        [HttpPut("{id}/skills")]
        public async Task<IActionResult> UpdateUserSkills(int id, [FromBody] IEnumerable<string> skillNames)
        {
            if (skillNames == null || !skillNames.Any())
                return BadRequest("Skill list is empty or missing.");

            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserSkills)
                .ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound($"User with ID {id} not found.");

            // Normalize incoming skill names
            var normalizedSkills = skillNames
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToLower())
                .ToList();

            // Load all existing skills for matching
            var existingSkills = await _context.Skills.ToListAsync();

            foreach (var skillName in normalizedSkills)
            {
                // Check if skill exists in DB
                var skill = existingSkills.FirstOrDefault(s => s.Name.ToLower() == skillName);

                // Create it if it doesn't exist
                if (skill == null)
                {
                    skill = new Skill
                    {
                        Name = skillName,
                        Level = "Beginner" // default level
                    };

                    _context.Skills.Add(skill);
                    await _context.SaveChangesAsync();

                    existingSkills.Add(skill);
                }

                // Check if user already has this skill linked
                bool alreadyLinked = user.PortfolioUserSkills
                    .Any(pus => pus.SkillId == skill.Id);

                if (!alreadyLinked)
                {
                    var link = new PortfolioUserSkill
                    {
                        PortfolioUserId = user.Id,
                        SkillId = skill.Id
                    };
                    _context.PortfolioUserSkills.Add(link);
                }
            }

            await _context.SaveChangesAsync();
            return Ok("User skills updated successfully.");
        }

        // POST: api/PortfolioUser
        [HttpPost]
        public async Task<ActionResult<PortfolioUserDto>> Create(PortfolioUserCreateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new PortfolioUser
            {
                Name = input.Name,
                Bio = input.Bio,
                ProfileImageUrl = input.ProfileImageUrl,
                Projects = input.Projects.Select(p => new Project
                {
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserId = p.PortfolioUserId
                }).ToList(),
                PortfolioUserSkills = input.PortforlioUserSkills.Select(s => new PortfolioUserSkill
                {
                   Skill = new Skill
                   {
                       Name = s.Name,
                       Level = s.Level
                   }
                }).ToList()
            };

            _context.PortfolioUsers.Add(user);
            await _context.SaveChangesAsync();

            var dto = new PortfolioUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                Projects = user.Projects.Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserId = p.PortfolioUserId
                }).ToList(),
                Skills = user.Skills.Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    PortfolioUsers = s.PortfolioUsers
                    // PortfolioUserId = s.PortfolioUserId
                }).ToList()
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        // PUT: api/PortfolioUser/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PortfolioUserCreateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.PortfolioUsers
                .Include(p => p.Projects)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // Update fields
            user.Name = input.Name;
            user.Bio = input.Bio;
            user.ProfileImageUrl = input.ProfileImageUrl;

            // Replace projects & skills (simple approach)
            _context.Projects.RemoveRange(user.Projects);
            _context.Skills.RemoveRange(user.Skills);

            user.Projects = input.Projects.Select(p => new Project
            {
                Title = p.Title,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                PortfolioUserId = id
            }).ToList();

            user.Skills = input.Skills.Select(s => new Skill
            {
                Name = s.Name,
                Level = s.Level,
                PortfolioUsers = s.PortfolioUsers
            }).ToList();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PortfolioUserExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/PortfolioUser/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.PortfolioUsers.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.PortfolioUsers.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PortfolioUserExists(int id)
        {
            return _context.PortfolioUsers.Any(e => e.Id == id);
        }
    }
}
