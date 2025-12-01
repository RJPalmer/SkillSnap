using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using SkillSnap_API.Data;
using SkillSnap_API.Services;
namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly SkillSnapDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SkillController> _logger;

        private const string SKILLS_CACHE_KEY = "skills:all";
        private const string SKILL_CACHE_KEY_PREFIX = "skill:";
        private const int CACHE_EXPIRATION_MINUTES = 30;

        public SkillController(
            SkillSnapDbContext context,
            ICacheService cacheService,
            ILogger<SkillController> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        // GET: api/Skill
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetAll()
        {
            _logger.LogInformation("Fetching all skills");
            return Ok(await _cacheService.GetOrSetAsync(
                SKILLS_CACHE_KEY,
                FetchAllSkillsFromDatabase,
                CACHE_EXPIRATION_MINUTES
            ));
        }

        /// <summary>
        /// Fetches all skills from the database without caching.
        /// </summary>
        private async Task<IEnumerable<SkillDto>> FetchAllSkillsFromDatabase()
        {
            var skills = await _context.Skills
                .Include(s => s.SkillPortfolioUsers)
                .AsNoTracking()
                .ToListAsync();

            return skills.Select(MapToSkillDto).ToList();
        }

        // GET: api/Skill/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<SkillDto>> GetById(int id)
        {
            _logger.LogInformation("Fetching skill {SkillId}", id);
            var skillDto = await _cacheService.GetOrSetAsync(
                $"{SKILL_CACHE_KEY_PREFIX}{id}",
                () => FetchSkillFromDatabase(id),
                CACHE_EXPIRATION_MINUTES
            );

            return skillDto == null ? NotFound() : Ok(skillDto);
        }

        /// <summary>
        /// Fetches a single skill from the database without caching.
        /// </summary>
        private async Task<SkillDto?> FetchSkillFromDatabase(int id)
        {
            var skill = await _context.Skills
                .Include(s => s.SkillPortfolioUsers)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            return skill == null ? null : MapToSkillDto(skill);
        }

        // POST: api/Skill
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SkillDto>> Create(SkillCreateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var skill = new Skill
                {
                    Name = input.Name,
                    Level = input.Level
                };

                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();

                // Invalidate all skills cache
                _cacheService.Remove(SKILLS_CACHE_KEY);
                _logger.LogInformation("Skill created with ID {SkillId}, cache invalidated", skill.Id);

                var dto = MapToSkillDto(skill);
                return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating skill");
                return StatusCode(500, "An error occurred while creating the skill.");
            }
        }

        // PUT: api/Skill/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, SkillCreateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
                return NotFound();

            skill.Name = input.Name;
            skill.Level = input.Level;

            try
            {
                await _context.SaveChangesAsync();

                // Invalidate caches for this skill and all skills
                _cacheService.Remove(SKILLS_CACHE_KEY);
                _cacheService.Remove($"{SKILL_CACHE_KEY_PREFIX}{id}");
                _logger.LogInformation("Skill {SkillId} updated, caches invalidated", id);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict updating skill {SkillId}", id);
                
                if (!_context.Skills.Any(e => e.Id == id))
                    return NotFound();
                
                return Conflict("The skill was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill {SkillId}", id);
                return StatusCode(500, "An error occurred while updating the skill.");
            }
        }

        // DELETE: api/Skill/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var skill = await _context.Skills
                    .Include(s => s.SkillPortfolioUsers)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (skill == null)
                    return NotFound();

                _context.PortfolioUserSkills.RemoveRange(skill.SkillPortfolioUsers);
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();

                // Invalidate caches
                _cacheService.Remove(SKILLS_CACHE_KEY);
                _cacheService.Remove($"{SKILL_CACHE_KEY_PREFIX}{id}");
                _logger.LogInformation("Skill {SkillId} deleted, caches invalidated", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skill {SkillId}", id);
                return StatusCode(500, "An error occurred while deleting the skill.");
            }
        }

        /// <summary>
        /// Maps a Skill entity to a SkillDto.
        /// </summary>
        private SkillDto MapToSkillDto(Skill skill)
        {
            return new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name,
                Level = skill.Level,
                SkillPortfolioUsers = skill.SkillPortfolioUsers.Select(pus => new PortfolioUserSkillDto
                {
                    PortfolioUserId = pus.PortfolioUserId,
                    SkillId = pus.SkillId
                }).ToList()
            };
        }
    }
}