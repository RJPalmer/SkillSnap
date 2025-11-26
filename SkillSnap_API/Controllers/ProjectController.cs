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

namespace SkillSnap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly SkillSnapDbContext _context;

        public ProjectController(SkillSnapDbContext context)
        {
            _context = context;
        }

        // GET: api/Project
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            return await _context.Projects.ToListAsync();
        }

        // GET: api/Project/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> PostProject(ProjectCreateDto newProject)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = new Project{Title = newProject.Title, Description = newProject.Description, ImageUrl = newProject.ImageUrl};    
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var dto = new ProjectDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                ImageUrl = project.ImageUrl

            };
            return CreatedAtAction("GetProject", new { id = dto.Id }, dto);
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
        public async Task<IActionResult> AttachProjectToUser([FromBody]PortfolioUserProjectCreateDto request)
        {
            // Check that both the user and project exist.
            var user = await _context.PortfolioUsers
                .Include(u => u.portfolioUserProjects).ThenInclude(pup => pup.Project)
                .FirstOrDefaultAsync(u => u.Id == request.PortfolioUserId);

            if (user == null)
                return NotFound($"User with ID {request.PortfolioUserId} not found.");

            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
                return NotFound($"Project with ID {request.ProjectId} not found.");
            
            //Add the project to the user’s list of projects
            if (user.portfolioUserProjects.Any(p => p.Project.Id == request.ProjectId))
                return Conflict($"Project with ID {request.ProjectId} is already attached to User {request.PortfolioUserId}.");

            user.portfolioUserProjects.Add(new PortfolioUserProject{
                PortfolioUser = user,
                // PortfolioUserId = user.Id,
                Project = project,
                // ProjectId = project.Id
            });

            await _context.SaveChangesAsync();

            return Ok($"Project {request.ProjectId} successfully attached to User {request.PortfolioUserId}.");
        }

    }
}
