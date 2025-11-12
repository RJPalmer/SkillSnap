using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using SkillSnap_API.Data;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly SkillSnapDbContext _context;

        public SkillController(SkillSnapDbContext context)
        {
            _context = context;
        }

        // GET: api/Skill
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetAll()
        {
            var skills = await _context.Skills.ToListAsync();
            var dtos = skills.Select(s => new SkillDto
            {
                Id = s.Id,
                Name = s.Name,
                Level = s.Level,
                PortfolioUsers = s.PortfolioUsers
                // PortfolioUserId = s.PortfolioUserId
            });
            return Ok(dtos);
        }

        // GET: api/Skill/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SkillDto>> GetById(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
                return NotFound();

            var dto = new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name,
                Level = skill.Level,
                PortfolioUsers = skill.PortfolioUsers
                // PortfolioUserId = skill.PortfolioUserId
            };

            return Ok(dto);
        }

        // POST: api/Skill
        [HttpPost]
        public async Task<ActionResult<SkillDto>> Create(SkillCreateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var skill = new Skill
            {
                Name = input.Name,
                Level = input.Level,
                PortfolioUsers = input.PortfolioUsers,
            };

            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            var dto = new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name,
                Level = skill.Level,
                PortfolioUsers = skill.PortfolioUsers
                // PortfolioUserId = skill.PortfolioUserId
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        // PUT: api/Skill/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SkillCreateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
                return NotFound();

            skill.Name = input.Name;
            skill.Level = input.Level;
            skill.PortfolioUsers = input.PortfolioUsers;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Skills.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Skill/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
                return NotFound();

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
