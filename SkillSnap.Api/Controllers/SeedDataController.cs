using Microsoft.AspNetCore.Mvc;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedDataController : ControllerBase
    {
        private readonly SkillSnapContext _context;

        public SeedDataController(SkillSnapContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Seed()
        {
            if (_context.PortfolioUsers.Any())
            {
                return BadRequest("Sample data already exists.");
            }

            var user = new PortfolioUser
            {
                Name = "Jordan Developer",
                Bio = "Full-stack developer passionate about learning new tech.",
                ProfileImageUrl = "https://example.com/image/jordan.png",
                Projects = new List<Project>
                {
                    new Project { Title = "Task Tracker", Description = "Manage tasks efficiently", ImageUrl = "..." },
                    new Project { Title = "Weather App", Description = "Forecast weather using APIs", ImageUrl = "..." }
                },
                Skills = new List<Skill>
                {
                    new Skill { Name = "C#", Level = "Advanced" },
                    new Skill { Name = "Blazor", Level = "Intermediate" }
                }
            };

            _context.PortfolioUsers.Add(user);
            _context.SaveChanges();
            return Ok("Sample data inserted.");
        }
    }
}