using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly SkillSnapContext _context;

        public ProjectsController(SkillSnapContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all projects with their associated portfolio user data
        /// </summary>
        /// <returns>List of all projects</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.PortfolioUser)
                    .ToListAsync();

                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving projects.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific project by ID
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.PortfolioUser)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return NotFound(new { message = $"Project with ID {id} not found." });
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the project.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="project">Project data</param>
        /// <returns>Created project</returns>
        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject(Project project)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify that the PortfolioUser exists
                var userExists = await _context.PortfolioUsers
                    .AnyAsync(u => u.Id == project.PortfolioUserId);

                if (!userExists)
                {
                    return BadRequest(new { message = $"Portfolio user with ID {project.PortfolioUserId} does not exist." });
                }

                // Set timestamps
                project.CreatedAt = DateTime.UtcNow;
                project.UpdatedAt = DateTime.UtcNow;

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Return the created project with user data
                var createdProject = await _context.Projects
                    .Include(p => p.PortfolioUser)
                    .FirstOrDefaultAsync(p => p.Id == project.Id);

                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, createdProject);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the project.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="project">Updated project data</param>
        /// <returns>Updated project</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<Project>> UpdateProject(int id, Project project)
        {
            try
            {
                if (id != project.Id)
                {
                    return BadRequest(new { message = "Project ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingProject = await _context.Projects.FindAsync(id);
                if (existingProject == null)
                {
                    return NotFound(new { message = $"Project with ID {id} not found." });
                }

                // Update properties
                existingProject.Title = project.Title;
                existingProject.Description = project.Description;
                existingProject.ImageUrl = project.ImageUrl;
                existingProject.GitHubUrl = project.GitHubUrl;
                existingProject.LiveDemoUrl = project.LiveDemoUrl;
                existingProject.Technologies = project.Technologies;
                existingProject.PortfolioUserId = project.PortfolioUserId;
                existingProject.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Return updated project with user data
                var updatedProject = await _context.Projects
                    .Include(p => p.PortfolioUser)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return Ok(updatedProject);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "The project was modified by another user. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the project.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project == null)
                {
                    return NotFound(new { message = $"Project with ID {id} not found." });
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Project '{project.Title}' deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the project.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all projects for a specific portfolio user
        /// </summary>
        /// <param name="userId">Portfolio User ID</param>
        /// <returns>List of projects for the user</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByUser(int userId)
        {
            try
            {
                var userExists = await _context.PortfolioUsers
                    .AnyAsync(u => u.Id == userId);

                if (!userExists)
                {
                    return NotFound(new { message = $"Portfolio user with ID {userId} not found." });
                }

                var projects = await _context.Projects
                    .Include(p => p.PortfolioUser)
                    .Where(p => p.PortfolioUserId == userId)
                    .ToListAsync();

                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user projects.", error = ex.Message });
            }
        }
    }
}