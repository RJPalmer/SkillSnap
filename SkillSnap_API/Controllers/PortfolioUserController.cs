using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.DTOs;
using SkillSnap.Shared.DTOs.Account;
using SkillSnap.Shared.Models;
using SkillSnap_API.Data;
using SkillSnap_API.Services;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioUserController : ControllerBase
    {
        private readonly SkillSnapDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenService _jwtTokenService;

        public PortfolioUserController(
            SkillSnapDbContext context,
            UserManager<ApplicationUser> userManager,
            JwtTokenService jwtTokenService)
        {
            _context = context;
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        // GET: api/PortfolioUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioUserDto>>> GetAll()
        {
            var users = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .AsNoTracking()
                .ToListAsync();

            var dtos = users.Select(MapToDto);
            return Ok(dtos);
        }

        // GET: api/PortfolioUser/unlinked
        // Returns portfolio users that are not linked to any ApplicationUser
        [Authorize]
        [HttpGet("unlinked")]
        public async Task<ActionResult<IEnumerable<PortfolioUserDto>>> GetUnlinkedPortfolioUsers()
        {
            var linkedIds = await _context.Users
                .Where(u => u.PortfolioUser != null)
                .Select(u => u.PortfolioUser!.Id)
                .ToListAsync();

            var unlinked = await _context.PortfolioUsers
                .Where(pu => !linkedIds.Contains(pu.Id))
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .AsNoTracking()
                .ToListAsync();

            return Ok(unlinked.Select(MapToDto));
        }

        // GET: api/PortfolioUser/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PortfolioUserDto>> GetById(int id)
        {
            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();
            return Ok(MapToDto(user));
        }

        // GET: api/PortfolioUser/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<PortfolioUserDto>> GetByName(string name)
        {
            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Name == name);

            if (user == null) return NotFound();
            return Ok(MapToDto(user));
        }

        // GET: api/PortfolioUser/{id}/projects
        [HttpGet("{id:int}/projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects(int id)
        {
            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var projectDtos = user.PortfolioUserProjects?
                .Where(p => p.Project != null)
                .Select(p => new ProjectDto
                {
                    Id = p.Project!.Id,
                    Title = p.Project.Title,
                    Description = p.Project.Description,
                    ImageUrl = p.Project.ImageUrl,
                    PortfolioUserId = p.PortfolioUser!.Id
                }) ?? Enumerable.Empty<ProjectDto>();

            return Ok(projectDtos);
        }

        // GET: api/PortfolioUser/{id}/skills
        [HttpGet("{id:int}/skills")]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetUserSkills(int id)
        {
            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var skillDtos = user.PortfolioUserSkills?
                .Where(pus => pus.Skill != null)
                .Select(pus => new SkillDto
                {
                    Id = pus.Skill!.Id,
                    Name = pus.Skill.Name,
                    Level = pus.Skill.Level
                })
                .ToList() ?? new List<SkillDto>();

            return Ok(skillDtos);
        }

        // PUT: api/PortfolioUser/{id}/skills
        // Accepts a list of skill names (strings). Adds new skills if needed and updates the join table.
        [HttpPut("{id:int}/skills")]
        public async Task<IActionResult> UpdateUserSkills(int id, [FromBody] IEnumerable<string> skillNames)
        {
            if (skillNames == null) return BadRequest("Skill list is missing.");

            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound($"User with ID {id} not found.");

            var normalized = skillNames
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Load all existing skills once
            var existingSkills = await _context.Skills.ToListAsync();

            // Remove skills no longer present
            var toRemove = user.PortfolioUserSkills
                .Where(pus => pus.Skill == null || !normalized.Any(n => string.Equals(n, pus.Skill.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (toRemove.Any()) _context.PortfolioUserSkills.RemoveRange(toRemove);

            // Add or attach skills
            foreach (var name in normalized)
            {
                var skill = existingSkills.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
                if (skill == null)
                {
                    skill = new Skill { Name = name, Level = "Beginner" };
                    _context.Skills.Add(skill);
                    await _context.SaveChangesAsync(); // persist to obtain Id
                    existingSkills.Add(skill);
                }

                if (!user.PortfolioUserSkills.Any(pus => pus.SkillId == skill.Id))
                {
                    user.PortfolioUserSkills.Add(new PortfolioUserSkill
                    {
                        PortfolioUserId = user.Id,
                        SkillId = skill.Id,
                        Skill = skill,
                        PortfolioUser = user
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "User skills updated successfully." });
        }

        // POST: api/PortfolioUser
        // Create a new PortfolioUser. This endpoint does NOT link to ApplicationUser automatically.
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<PortfolioUserDto>> Create([FromBody] PortfolioUserCreateDto input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new PortfolioUser
            {
                Name = input.Name?.Trim() ?? string.Empty,
                Bio = input.Bio?.Trim() ?? string.Empty,
                ProfileImageUrl = input.ProfileImageUrl?.Trim() ?? string.Empty
            };

            if (input.PortfolioUserSkills != null && input.PortfolioUserSkills.Any())
            {
                var skillNames = input.PortfolioUserSkills
                    .Where(s => s.Skill != null && !string.IsNullOrWhiteSpace(s.Skill.Name))
                    .Select(s => s.Skill!.Name.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var existingSkills = await _context.Skills.ToListAsync();

                foreach (var skillName in skillNames)
                {
                    var skill = existingSkills.FirstOrDefault(s => string.Equals(s.Name, skillName, StringComparison.OrdinalIgnoreCase));
                    if (skill == null)
                    {
                        skill = new Skill { Name = skillName, Level = "Beginner" };
                        _context.Skills.Add(skill);
                        await _context.SaveChangesAsync();
                        existingSkills.Add(skill);
                    }

                    user.PortfolioUserSkills.Add(new PortfolioUserSkill
                    {
                        SkillId = skill.Id,
                        Skill = skill,
                        PortfolioUser = user
                    });
                }
            }

            _context.PortfolioUsers.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapToDto(user));
        }

        // PUT: api/PortfolioUser/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PortfolioUserCreateDto input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            user.Name = input.Name?.Trim() ?? user.Name;
            user.Bio = input.Bio?.Trim() ?? user.Bio;
            user.ProfileImageUrl = input.ProfileImageUrl?.Trim() ?? user.ProfileImageUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PortfolioUserExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/PortfolioUser/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.PortfolioUsers.FindAsync(id);
            if (user == null) return NotFound();

            _context.PortfolioUsers.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/PortfolioUser/me
        // Returns the PortfolioUser tied to the authenticated ApplicationUser via the portfolioUserId claim
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var portfolioUserIdClaim = User.FindFirst("portfolioUserId")?.Value;

            if (string.IsNullOrWhiteSpace(portfolioUserIdClaim))
                return Unauthorized("portfolioUserId claim missing.");

            if (!int.TryParse(portfolioUserIdClaim, out var portfolioUserId))
                return BadRequest("Invalid portfolioUserId claim value.");

            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == portfolioUserId);

            if (user == null) return NotFound("Portfolio user not found.");

            return Ok(MapToDto(user));
        }

        // POST: api/PortfolioUser/link/{portfolioUserId}
        // Links the currently authenticated ApplicationUser to an existing PortfolioUser and returns a refreshed JWT
        [Authorize]
        [HttpPost("link/{portfolioUserId:int}")]
        public async Task<IActionResult> LinkPortfolioUser(int portfolioUserId)
        {
            // validate portfolio user exists
            var portfolioUser = await _context.PortfolioUsers.FindAsync(portfolioUserId);
            if (portfolioUser == null) return NotFound("PortfolioUser not found.");

            // get current application user id from claims
            var appUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(appUserId)) return Unauthorized();

            var appUser = await _userManager.FindByIdAsync(appUserId);
            if (appUser == null) return Unauthorized();

            // prevent linking if already linked to a different portfolio user
            if (appUser.PortfolioUser != null && appUser.PortfolioUser.Id != portfolioUserId)
            {
                return BadRequest("This account is already linked to a different portfolio user.");
            }

            appUser.PortfolioUser = portfolioUser;
            var updateResult = await _userManager.UpdateAsync(appUser);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new { errors = updateResult.Errors.Select(e => e.Description) });
            }

            // Optionally regenerate token containing the new claim
            string? token = null;
            try
            {
                token = await _jwtTokenService.GenerateToken(appUser);
            }
            catch
            {
                // token generation failure shouldn't block linking; log if you have a logger
            }

            return Ok(new LinkPortfolioUserResponseDto { Token = token ?? string.Empty, PortfolioUserId = portfolioUserId });
        }

        private PortfolioUserDto MapToDto(PortfolioUser user)
        {
            return new PortfolioUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl!,
                Projects = user.PortfolioUserProjects?.Where(p => p.Project != null).Select(p => new PortfolioUserProjectDto
                {
                    PortfolioUserId = p.PortfolioUser!.Id,
                    ProjectId = p.Project!.Id,
                    Project = p.Project == null ? null : new ProjectDto
                    {
                        Id = p.Project.Id,
                        Title = p.Project.Title,
                        Description = p.Project.Description,
                        ImageUrl = p.Project.ImageUrl
                    }
                }).ToList() ?? new List<PortfolioUserProjectDto>(),

                PortfolioUserSkills = user.PortfolioUserSkills?.Where(pus => pus.Skill != null).Select(pus => new PortfolioUserSkillDto
                {
                    SkillId = pus.Skill!.Id,
                    PortfolioUserId = pus.PortfolioUser!.Id,
                    Skill = pus.Skill == null ? null : new SkillDto
                    {
                        Id = pus.Skill.Id,
                        Name = pus.Skill.Name,
                        Level = pus.Skill.Level
                    }
                }).ToList() ?? new List<PortfolioUserSkillDto>()
            };
        }

        private bool PortfolioUserExists(int id)
        {
            return _context.PortfolioUsers.Any(e => e.Id == id);
        }
    }
}