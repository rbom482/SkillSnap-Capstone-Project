using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : ControllerBase
    {
        private readonly SkillSnapContext _context;

        public SkillsController(SkillSnapContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all skills with their associated portfolio user data
        /// </summary>
        /// <returns>List of all skills</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
        {
            try
            {
                var skills = await _context.Skills
                    .Include(s => s.PortfolioUser)
                    .ToListAsync();

                return Ok(skills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving skills.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific skill by ID
        /// </summary>
        /// <param name="id">Skill ID</param>
        /// <returns>Skill details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Skill>> GetSkill(int id)
        {
            try
            {
                var skill = await _context.Skills
                    .Include(s => s.PortfolioUser)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (skill == null)
                {
                    return NotFound(new { message = $"Skill with ID {id} not found." });
                }

                return Ok(skill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the skill.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new skill
        /// </summary>
        /// <param name="skill">Skill data</param>
        /// <returns>Created skill</returns>
        [HttpPost]
        public async Task<ActionResult<Skill>> CreateSkill(Skill skill)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify that the PortfolioUser exists
                var userExists = await _context.PortfolioUsers
                    .AnyAsync(u => u.Id == skill.PortfolioUserId);

                if (!userExists)
                {
                    return BadRequest(new { message = $"Portfolio user with ID {skill.PortfolioUserId} does not exist." });
                }

                // Validate skill level
                var validLevels = new[] { "Beginner", "Intermediate", "Advanced", "Expert" };
                if (!validLevels.Contains(skill.Level))
                {
                    return BadRequest(new { message = "Skill level must be one of: Beginner, Intermediate, Advanced, Expert" });
                }

                // Check for duplicate skills for the same user
                var existingSkill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == skill.Name.ToLower() && 
                                            s.PortfolioUserId == skill.PortfolioUserId);

                if (existingSkill != null)
                {
                    return Conflict(new { message = $"Skill '{skill.Name}' already exists for this user." });
                }

                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();

                // Return the created skill with user data
                var createdSkill = await _context.Skills
                    .Include(s => s.PortfolioUser)
                    .FirstOrDefaultAsync(s => s.Id == skill.Id);

                return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, createdSkill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the skill.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing skill
        /// </summary>
        /// <param name="id">Skill ID</param>
        /// <param name="skill">Updated skill data</param>
        /// <returns>Updated skill</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<Skill>> UpdateSkill(int id, Skill skill)
        {
            try
            {
                if (id != skill.Id)
                {
                    return BadRequest(new { message = "Skill ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate skill level
                var validLevels = new[] { "Beginner", "Intermediate", "Advanced", "Expert" };
                if (!validLevels.Contains(skill.Level))
                {
                    return BadRequest(new { message = "Skill level must be one of: Beginner, Intermediate, Advanced, Expert" });
                }

                var existingSkill = await _context.Skills.FindAsync(id);
                if (existingSkill == null)
                {
                    return NotFound(new { message = $"Skill with ID {id} not found." });
                }

                // Check for duplicate skill names (excluding current skill)
                var duplicateSkill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == skill.Name.ToLower() && 
                                            s.PortfolioUserId == skill.PortfolioUserId && 
                                            s.Id != id);

                if (duplicateSkill != null)
                {
                    return Conflict(new { message = $"Skill '{skill.Name}' already exists for this user." });
                }

                // Update properties
                existingSkill.Name = skill.Name;
                existingSkill.Level = skill.Level;
                existingSkill.PortfolioUserId = skill.PortfolioUserId;

                await _context.SaveChangesAsync();

                // Return updated skill with user data
                var updatedSkill = await _context.Skills
                    .Include(s => s.PortfolioUser)
                    .FirstOrDefaultAsync(s => s.Id == id);

                return Ok(updatedSkill);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "The skill was modified by another user. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the skill.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a skill
        /// </summary>
        /// <param name="id">Skill ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            try
            {
                var skill = await _context.Skills.FindAsync(id);
                if (skill == null)
                {
                    return NotFound(new { message = $"Skill with ID {id} not found." });
                }

                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Skill '{skill.Name}' deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the skill.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all skills for a specific portfolio user
        /// </summary>
        /// <param name="userId">Portfolio User ID</param>
        /// <returns>List of skills for the user</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Skill>>> GetSkillsByUser(int userId)
        {
            try
            {
                var userExists = await _context.PortfolioUsers
                    .AnyAsync(u => u.Id == userId);

                if (!userExists)
                {
                    return NotFound(new { message = $"Portfolio user with ID {userId} not found." });
                }

                var skills = await _context.Skills
                    .Include(s => s.PortfolioUser)
                    .Where(s => s.PortfolioUserId == userId)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                return Ok(skills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user skills.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get skills grouped by level for a specific user
        /// </summary>
        /// <param name="userId">Portfolio User ID</param>
        /// <returns>Skills grouped by proficiency level</returns>
        [HttpGet("user/{userId}/by-level")]
        public async Task<ActionResult<object>> GetSkillsByUserGroupedByLevel(int userId)
        {
            try
            {
                var userExists = await _context.PortfolioUsers
                    .AnyAsync(u => u.Id == userId);

                if (!userExists)
                {
                    return NotFound(new { message = $"Portfolio user with ID {userId} not found." });
                }

                var skills = await _context.Skills
                    .Where(s => s.PortfolioUserId == userId)
                    .GroupBy(s => s.Level)
                    .Select(g => new
                    {
                        Level = g.Key,
                        Count = g.Count(),
                        Skills = g.OrderBy(s => s.Name).ToList()
                    })
                    .OrderBy(x => Array.IndexOf(new[] { "Beginner", "Intermediate", "Advanced", "Expert" }, x.Level))
                    .ToListAsync();

                return Ok(skills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving grouped skills.", error = ex.Message });
            }
        }
    }
}