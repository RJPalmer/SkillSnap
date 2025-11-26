using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using SkillSnap_API.Data;
using NuGet.Protocol;
using Microsoft.AspNetCore.Authorization;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioUserController : ControllerBase
    {
        private readonly SkillSnapDbContext _context;
        //private object value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public PortfolioUserController(SkillSnapDbContext context)
        {
            _context = context;
        }



        // GET: api/PortfolioUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioUserDto>>> GetAll()
        {
            var users = await _context.PortfolioUsers
                .Include(u => u.portfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .ToListAsync();

            var dtos = users.Select(u => new PortfolioUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                Projects = u.portfolioUserProjects.Select(p => new PortfolioUserProjectDto
                {
                    ProjectId = p.Project.Id,
                    PortfolioUserId = p.PortfolioUser.Id
                }).ToList(),

                PortfolioUserSkills = u.PortfolioUserSkills.Select(pus => new PortfolioUserSkillDto
                {
                    SkillId = pus.Skill.Id,
                    PortfolioUser = pus.PortfolioUser != null ? new PortfolioUserDto
                    {
                        Id = pus.PortfolioUser.Id,
                        Name = pus.PortfolioUser.Name,
                        Bio = pus.PortfolioUser.Bio,
                        ProfileImageUrl = pus.PortfolioUser.ProfileImageUrl
                    } : null,
                    Skill = pus.Skill != null ? new SkillDto
                    {
                        Id = pus.Skill.Id,
                        Name = pus.Skill.Name,
                        Level = pus.Skill.Level
                    } : null
                }).ToList()
            });

            return Ok(dtos);
        }

        // GET: api/PortfolioUser/unlinked
        [Authorize]
        [HttpGet("unlinked")]
        public async Task<ActionResult<IEnumerable<PortfolioUserDto>>> GetUnlinkedPortfolioUsers()
        {
            // Get all PortfolioUsers that do NOT have an associated ApplicationUser
            var unlinkedUsers = await _context.PortfolioUsers
                .Where(pu => !_context.Users.Any(au => au.PortfolioUser != null && au.PortfolioUser.Id == pu.Id))
                .Include(u => u.portfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .ToListAsync();

            var dtos = unlinkedUsers.Select(u => new PortfolioUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                Projects = u.portfolioUserProjects.Select(p => new PortfolioUserProjectDto
                {
                    ProjectId = p.Project.Id,
                    PortfolioUserId = p.PortfolioUser.Id
                }).ToList(),
                PortfolioUserSkills = u.PortfolioUserSkills.Select(pus => new PortfolioUserSkillDto
                {
                    SkillId = pus.Skill.Id,
                    PortfolioUser = pus.PortfolioUser != null ? new PortfolioUserDto
                    {
                        Id = pus.PortfolioUser.Id,
                        Name = pus.PortfolioUser.Name,
                        Bio = pus.PortfolioUser.Bio,
                        ProfileImageUrl = pus.PortfolioUser.ProfileImageUrl
                    } : null,
                    Skill = pus.Skill != null ? new SkillDto
                    {
                        Id = pus.Skill.Id,
                        Name = pus.Skill.Name,
                        Level = pus.Skill.Level
                    } : null
                }).ToList()
            });

            return Ok(dtos);
        }

        // GET: api/PortfolioUser/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PortfolioUserDto>> GetById(int id)
        {
            var user = await _context.PortfolioUsers
                .Include(u => u.portfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var dto = new PortfolioUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                Projects = user.portfolioUserProjects.Select(p => new PortfolioUserProjectDto
                {

                    Project = new ProjectDto
                    {
                        Id = p.Project.Id,
                        Title = p.Project.Title,
                        Description = p.Project.Description,
                        ImageUrl = p.Project.ImageUrl
                    },
                    PortfolioUser = new PortfolioUserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Bio = user.Bio,
                        ProfileImageUrl = user.ProfileImageUrl
                    },
                    PortfolioUserId = p.PortfolioUser.Id,
                    ProjectId = p.Project.Id
                }).ToList(),
                PortfolioUserSkills = user.PortfolioUserSkills.Select(pus => new PortfolioUserSkillDto
                {
                    SkillId = pus.Skill.Id,
                    PortfolioUserId = pus.PortfolioUser.Id,
                    Skill = new SkillDto
                    {
                        Id = pus.Skill.Id,
                        Name = pus.Skill.Name,
                        Level = pus.Skill.Level
                    }
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/PortfolioUser/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<PortfolioUserDto>> GetByName(string name)
        {
            var user = await _context.PortfolioUsers
                .Include(u => u.portfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Name == name);

            if (user == null)
                return NotFound();

            var dto = new PortfolioUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                Projects = user.portfolioUserProjects.Select(p => new PortfolioUserProjectDto
                {
                    PortfolioUserId = user.Id,
                    PortfolioUser = new PortfolioUserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Bio = user.Bio,
                        ProfileImageUrl = user.ProfileImageUrl
                    },
                    Project = new ProjectDto
                    {
                        Id = p.Project.Id,
                        Title = p.Project.Title,
                        Description = p.Project.Description,
                        ImageUrl = p.Project.ImageUrl
                    }
                }).ToList(),
                PortfolioUserSkills = user.PortfolioUserSkills.Select(pus => new PortfolioUserSkillDto
                {
                    SkillId = pus.Skill.Id,
                    PortfolioUserId = pus.PortfolioUser.Id,
                    Skill = new SkillDto
                    {
                        Id = pus.Skill.Id,
                        Name = pus.Skill.Name,
                        Level = pus.Skill.Level
                    }
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/PortfolioUser/{id}/projects
        [HttpGet("{id}/projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects(int id)
        {
            var user = await _context.PortfolioUsers
                .Include(u => u.portfolioUserProjects).ThenInclude(pup => pup.Project)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var projectDtos = user.portfolioUserProjects.Select(p => new ProjectDto
            {
                Id = p.Project.Id,
                Title = p.Project.Title,
                Description = p.Project.Description,
                ImageUrl = p.Project.ImageUrl,
                PortfolioUserId = p.PortfolioUser.Id
            });

            return Ok(projectDtos);
        }

        // GET: api/PortfolioUser/{id}/skills
        [HttpGet("{id}/skills")]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetUserSkills(int id)
        {
            // get the user info with skills
            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            if (user.PortfolioUserSkills == null || !user.PortfolioUserSkills.Any())
                return Ok(Enumerable.Empty<SkillDto>());

            var skillDtos = user.PortfolioUserSkills
                .Where(pus => pus.Skill != null)
                .Select(pus => new SkillDto
                {
                    Id = pus.Skill!.Id,
                    Name = pus.Skill!.Name,
                    Level = pus.Skill!.Level,
                })
                .ToList();

            return Ok(skillDtos);
        }

        // PUT: api/PortfolioUser/{id}/skills
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
                .Distinct()
                .ToList();

            // Load all existing skills for matching
            var existingSkills = await _context.Skills.ToListAsync();

            // Remove any PortfolioUserSkills not in the new list
            var skillsToRemove = user.PortfolioUserSkills
                .Where(pus => !normalizedSkills.Contains(pus.Skill.Name.ToLower()))
                .ToList();
            _context.PortfolioUserSkills.RemoveRange(skillsToRemove);

            foreach (var skillName in normalizedSkills)
            {
                var skill = existingSkills.FirstOrDefault(s => s.Name.ToLower() == skillName);

                if (skill == null)
                {
                    skill = new Skill
                    {
                        Name = skillName,
                        Level = "Beginner"
                    };
                    _context.Skills.Add(skill);
                    await _context.SaveChangesAsync();
                    existingSkills.Add(skill);
                }

                if (!user.PortfolioUserSkills.Any(pus => pus.Skill.Id == skill.Id))
                {
                    user.PortfolioUserSkills.Add(new PortfolioUserSkill
                    {
                        // PortfolioUser.Id = user.Id,
                        PortfolioUser = user,
                        // S/killId = skill.Id,
                        Skill = skill

                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok("User skills updated successfully.");
        }

        // POST: api/PortfolioUser
        [Authorize]
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

            };
            if (input.PortfolioUserSkills != null && input.PortfolioUserSkills.Any())
            {
                var skillNames = input.PortfolioUserSkills
                    .Where(s => s.Skill != null && !string.IsNullOrWhiteSpace(s.Skill.Name))
                    .Select(s => s.Skill!.Name.Trim().ToLower())
                    .Distinct()
                     .ToList();

                var existingSkills = await _context.Skills.ToListAsync();

                foreach (var skillName in skillNames)
                {
                    var skill = existingSkills.FirstOrDefault(s => s.Name.ToLower() == skillName)
                                ?? new Skill { Name = skillName, Level = "Beginner" };
                    if (!existingSkills.Contains(skill))
                    {
                        _context.Skills.Add(skill);
                        await _context.SaveChangesAsync();
                        existingSkills.Add(skill);
                    }

                    user.PortfolioUserSkills.Add(new PortfolioUserSkill
                    {
                        // SkillId = skill.Id,
                        Skill = skill,
                        // PortfolioUserId = user.Id,
                        PortfolioUser = user
                    });
                }
            }

            _context.PortfolioUsers.Add(user);
            await _context.SaveChangesAsync();

            // Link the ApplicationUser to the newly created PortfolioUser
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (appUser != null)
                {
                    
                }
            }

            var dto = new PortfolioUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                Projects = user.portfolioUserProjects.Select(p => new PortfolioUserProjectDto
                {
                    PortfolioUserId = p.PortfolioUser.Id,
                    ProjectId = p.Project.Id
                }).ToList(),
                PortfolioUserSkills = user.PortfolioUserSkills.Select(pus => new PortfolioUserSkillDto
                {
                    SkillId = pus.Skill.Id,
                    PortfolioUserId = pus.PortfolioUser.Id,
                    Skill = new SkillDto
                    {
                        Id = pus.Skill.Id,
                        Name = pus.Skill.Name,
                        Level = pus.Skill.Level
                    }
                }).ToList()
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        // PUT: api/PortfolioUser/{id}  
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PortfolioUserCreateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.PortfolioUsers
                .Include(u => u.portfolioUserProjects).ThenInclude(pup => pup.Project)
                .Include(u => u.PortfolioUserSkills).ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            user.Name = input.Name;
            user.Bio = input.Bio;
            user.ProfileImageUrl = input.ProfileImageUrl;

            /*
            _context.Projects.RemoveRange(user.Projects);
            user.Projects = input.Projects?.Select(p => new Project
            {
                Title = p.Title,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                PortfolioUserId = id
            }).ToList() ?? new List<Project>();

            var skillNames = input.PortfolioUserSkills?.Where(s => s.Skill != null && !string.IsNullOrWhiteSpace(s.Skill.Name))
                .Select(s => s.Skill.Name.Trim().ToLower())
                .Distinct()
                .ToList() ?? new List<string>();

            var existingSkills = await _context.Skills.ToListAsync();

            var skillsToRemove = user.PortfolioUserSkills
                .Where(pus => !skillNames.Contains(pus.Skill.Name.ToLower()))
                .ToList();
            _context.PortfolioUserSkills.RemoveRange(skillsToRemove);

            foreach (var skillName in skillNames)
            {
                var skill = existingSkills.FirstOrDefault(s => s.Name.ToLower() == skillName)
                            ?? new Skill { Name = skillName, Level = "Beginner" };
                if (!existingSkills.Contains(skill))
                {
                    _context.Skills.Add(skill);
                    await _context.SaveChangesAsync();
                    existingSkills.Add(skill);
                }

                if (!user.PortfolioUserSkills.Any(pus => pus.SkillId == skill.Id))
                {
                    user.PortfolioUserSkills.Add(new PortfolioUserSkill
                    {
                        SkillId = skill.Id,
                        PortfolioUserId = id,
                        Skill = skill,
                        PortfolioUser = user
                    });
                }
            }
            */
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PortfolioUserExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/PortfolioUser/{id}
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
                .Include(u => u.PortfolioUserSkills)
                    .ThenInclude(pus => pus.Skill)
                .Include(u => u.portfolioUserProjects)
                    .ThenInclude(pup => pup.Project)
                .FirstOrDefaultAsync(u => u.Id == portfolioUserId);

            if (user == null)
                return NotFound("Portfolio user not found.");

            // Map to DTO to avoid exposing EF entities
            var dto = new PortfolioUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                Projects = user.portfolioUserProjects.Select(p => new PortfolioUserProjectDto
                {
                    PortfolioUserId = p.PortfolioUser.Id,
                    ProjectId = p.Project.Id,
                    Project = p.Project == null ? null : new ProjectDto
                    {
                        Id = p.Project.Id,
                        Title = p.Project.Title,
                        Description = p.Project.Description,
                        ImageUrl = p.Project.ImageUrl
                    }
                }).ToList(),
                PortfolioUserSkills = user.PortfolioUserSkills.Select(pus => new PortfolioUserSkillDto
                {
                    SkillId = pus.Skill.Id,
                    PortfolioUserId = pus.PortfolioUser.Id,
                    Skill = pus.Skill == null ? null : new SkillDto
                    {
                        Id = pus.Skill.Id,
                        Name = pus.Skill.Name,
                        Level = pus.Skill.Level
                    }
                }).ToList()
            };

            return Ok(dto);
        }
        private bool PortfolioUserExists(int id)
        {
            return _context.PortfolioUsers.Any(e => e.Id == id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int? GetAuthenticatedPortfolioUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "portfolioUserId");
            if (claim == null)
                return null;

            if (int.TryParse(claim.Value, out int id))
                return id;

            return null;
        }
    }
}