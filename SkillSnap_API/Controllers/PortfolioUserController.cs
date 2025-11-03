using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.models;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioUserController : Controller
    {
        private readonly SkillSnapDBContext _context;

        public PortfolioUserController(SkillSnapDBContext context)
        {
            _context = context;
        }

        // GET: PortfolioUser
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.PortfolioUser.ToListAsync());
        }

        // GET: PortfolioUser/Details/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioUser = await _context.PortfolioUser
                .FirstOrDefaultAsync(m => m.Id == id);
            if (portfolioUser == null)
            {
                return NotFound();
            }

            return View(portfolioUser);
        }

        // GET: PortfolioUser/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PortfolioUser/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Bio,ProfileImageUrl")] PortfolioUser portfolioUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(portfolioUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(portfolioUser);
        }

        // GET: PortfolioUser/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioUser = await _context.PortfolioUser.FindAsync(id);
            if (portfolioUser == null)
            {
                return NotFound();
            }
            return View(portfolioUser);
        }

        // POST: PortfolioUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Bio,ProfileImageUrl")] PortfolioUser portfolioUser)
        {
            if (id != portfolioUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
                return RedirectToAction(nameof(Index));
            }
            return View(portfolioUser);
        }

        // GET: PortfolioUser/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioUser = await _context.PortfolioUser
                .FirstOrDefaultAsync(m => m.Id == id);
            if (portfolioUser == null)
            {
                return NotFound();
            }

            return View(portfolioUser);
        }

        // POST: PortfolioUser/Delete/5
        [HttpPost, ActionName("Delete"), Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var portfolioUser = await _context.PortfolioUser.FindAsync(id);
            if (portfolioUser != null)
            {
                _context.PortfolioUser.Remove(portfolioUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PortfolioUserExists(int id)
        {
            return _context.PortfolioUser.Any(e => e.Id == id);
        }
    }
}
