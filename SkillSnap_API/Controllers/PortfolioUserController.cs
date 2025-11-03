using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap_API.Data;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioUserController : Controller
    {
        private readonly SkillSnapDbContext _context;

        public PortfolioUserController(SkillSnapDbContext context)
        {
            _context = context;
        }

        // GET: api/PortfolioUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioUser>>> Index()
        {
            return await _context.PortfolioUsers.Include(p => p.Projects)
                                              .Include(p => p.Skills)
                                              .ToListAsync();
        }

        // GET: PortfolioUser/Details/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PortfolioUser>> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioUser = await _context.PortfolioUsers
                .Include(p => p.Projects)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (portfolioUser == null)
            {
                return NotFound();
            }

            return Ok(portfolioUser);
        }

        // GET: PortfolioUser/Create
        // POST: api/PortfolioUser
        [HttpPost]
        public async Task<ActionResult<PortfolioUser>> Create(PortfolioUser portfolioUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.PortfolioUsers.Add(portfolioUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Details), new { id = portfolioUser.Id }, portfolioUser);
        }

        // PUT: api/PortfolioUser/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, PortfolioUser portfolioUser)
        {
            if (id != portfolioUser.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Update(portfolioUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PortfolioUserExists(portfolioUser.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/PortfolioUser/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var portfolioUser = await _context.PortfolioUsers.FindAsync(id);
            if (portfolioUser == null)
            {
                return NotFound();
            }

            _context.PortfolioUsers.Remove(portfolioUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PortfolioUserExists(int id)
        {
            return _context.PortfolioUsers.Any(e => e.Id == id);
        }
    }
}
