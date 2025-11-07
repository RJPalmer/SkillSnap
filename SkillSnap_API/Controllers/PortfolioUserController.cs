using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using SkillSnap_API.Data;

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
                .Include(p => p.Skills)
                .ToListAsync();

            var dtos = users.Select(u => new PortfolioUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                Projects = u.Projects.Select(p => new ProjectDto {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserId = p.PortfolioUserId
                }).ToList(),
                Skills = u.Skills.Select(s => new SkillDto {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    PortfolioUserId = s.PortfolioUserId
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
                    PortfolioUserId = s.PortfolioUserId
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
                    PortfolioUserId = s.PortfolioUserId
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
                PortfolioUserId = s.PortfolioUserId
            });

            return Ok(skillDtos);
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
                Projects = input.Projects.Select(p => new Project {
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserId = p.PortfolioUserId
                }).ToList(),
                Skills = input.Skills.Select(s => new Skill {
                    Name = s.Name,
                    Level = s.Level,
                    PortfolioUserId = s.PortfolioUserId
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
                Projects = user.Projects.Select(p => new ProjectDto {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserId = p.PortfolioUserId
                }).ToList(),
                Skills = user.Skills.Select(s => new SkillDto {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    PortfolioUserId = s.PortfolioUserId
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

            user.Projects = input.Projects.Select(p => new Project {
                Title = p.Title,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                PortfolioUserId = id
            }).ToList();

            user.Skills = input.Skills.Select(s => new Skill {
                Name = s.Name,
                Level = s.Level,
                PortfolioUserId = id
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
