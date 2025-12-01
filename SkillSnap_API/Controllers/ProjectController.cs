using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.DTOs;
using SkillSnap.Shared.Models;
using SkillSnap_API.Data;
using SkillSnap_API.Services;
using Microsoft.Extensions.Logging;

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly SkillSnapDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ProjectController> _logger;

        private const string PROJECTS_CACHE_KEY = "projects:all";
        private const string PROJECT_CACHE_KEY_PREFIX = "project:";
        private const int CACHE_MINUTES = 30;

        public ProjectController(SkillSnapDbContext context, ICacheService cacheService, ILogger<ProjectController> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        // GET: api/Project
        [HttpGet]
        public async Task<IEnumerable<ProjectDto>> GetProjects(int page = 1, int pageSize = 100)
        {
            // Cache per-page to avoid returning excessively large lists and to make invalidation manageable
            var cacheKey = $"projects:page:{page}:size:{pageSize}";
            var projects = await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _context.Projects
                    .AsNoTracking()
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProjectDto { Id = p.Id, Title = p.Title, Description = p.Description, ImageUrl = p.ImageUrl })
                    .ToListAsync();
            }, CACHE_MINUTES);

            return projects;
        }

        // GET: api/Project/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            var cacheKey = $"{PROJECT_CACHE_KEY_PREFIX}{id}";
            var dto = await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var p = await _context.Projects.AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => new ProjectDto { Id = x.Id, Title = x.Title, Description = x.Description, ImageUrl = x.ImageUrl })
                    .FirstOrDefaultAsync();
                return p;
            }, CACHE_MINUTES);

            if (dto == null) return NotFound();
            return dto;
        }

        // PUT: api/Project/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest();
            }

            _context.Projects.Update(project);
            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        // POST: api/Project
        /// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> PostProject(ProjectCreateDto newProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extract PortfolioUserId from JWT (UserContext equivalent on server)
            var portfolioUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == "portfolioUserId")?.Value;
            if (string.IsNullOrEmpty(portfolioUserIdClaim))
                return Unauthorized("No PortfolioUserId claim found.");

            if (!int.TryParse(portfolioUserIdClaim, out var portfolioUserId))
                return Unauthorized("Invalid PortfolioUserId claim.");

            // Ensure PortfolioUser exists
            var portfolioUser = await _context.PortfolioUsers
                .Include(pu => pu.PortfolioUserProjects)
                .FirstOrDefaultAsync(pu => pu.Id == portfolioUserId);

            if (portfolioUser == null)
                return NotFound($"PortfolioUser with ID {portfolioUserId} not found.");

            // Create project
            var project = new Project
            {
                Title = newProject.Title,
                Description = newProject.Description,
                ImageUrl = newProject.ImageUrl
            };

            // Create project and link in a single unit of work to minimize DB roundtrips
            _context.Projects.Add(project);
            _context.PortfolioUserProjects.Add(new PortfolioUserProject
            {
                PortfolioUserId = portfolioUser.Id,
                Project = project
            });
            await _context.SaveChangesAsync();

            // Invalidate project caches
            _cacheService.Remove(PROJECTS_CACHE_KEY);
            _cacheService.Remove($"{PROJECT_CACHE_KEY_PREFIX}{project.Id}");

            // Build DTO response
            var dto = new ProjectDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                ImageUrl = project.ImageUrl
            };

            return CreatedAtAction(nameof(GetProject), new { id = dto.Id }, dto);
        }

        // DELETE: api/Project/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            // Invalidate caches
            _cacheService.Remove(PROJECTS_CACHE_KEY);
            _cacheService.Remove($"{PROJECT_CACHE_KEY_PREFIX}{id}");

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        /// <summary>
        /// AttachProjectToUser
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpPost("attach")]
        public async Task<IActionResult> AttachProjectToUser([FromBody] PortfolioUserProjectCreateDto request)
        {
            // Check that both the user and project exist.
            var user = await _context.PortfolioUsers
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .FirstOrDefaultAsync(u => u.Id == request.PortfolioUserId);

            if (user == null)
                return NotFound($"User with ID {request.PortfolioUserId} not found.");

            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
                return NotFound($"Project with ID {request.ProjectId} not found.");

            // Attach in transaction to avoid races
            // Attempt to start a transaction; if the provider (InMemory) doesn't support transactions,
            // fall back to executing without a transaction to keep tests working.
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? tx = null;
            try
            {
                try
                {
                    // Avoid starting a transaction when using the InMemory provider (it does not support transactions).
                    var provider = _context.Database.ProviderName ?? string.Empty;
                    if (!provider.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
                    {
                        tx = await _context.Database.BeginTransactionAsync();
                    }
                    else
                    {
                        tx = null;
                    }
                }
                catch (InvalidOperationException)
                {
                    // If anything goes wrong attempting to start a transaction, fall back to no transaction.
                    tx = null;
                }
                // Re-check existence in DB to avoid race
                var exists = await _context.PortfolioUserProjects.AnyAsync(p => p.PortfolioUserId == request.PortfolioUserId && p.ProjectId == request.ProjectId);
                if (exists)
                {
                    return Conflict($"Project with ID {request.ProjectId} is already attached to User {request.PortfolioUserId}.");
                }

                _context.PortfolioUserProjects.Add(new PortfolioUserProject
                {
                    PortfolioUserId = user.Id,
                    ProjectId = project.Id
                });

                await _context.SaveChangesAsync();
                if (tx != null)
                    await tx.CommitAsync();

                // Invalidate cache entries related to projects and user's project list
                _cacheService.Remove(PROJECTS_CACHE_KEY);
                _cacheService.Remove($"user:{user.Id}:projects");

                return Ok($"Project {request.ProjectId} successfully attached to User {request.PortfolioUserId}.");
            }
            catch
            {
                if (tx != null)
                {
                    await tx.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (tx != null)
                {
                    await tx.DisposeAsync();
                }
            }
        }

    }
}
