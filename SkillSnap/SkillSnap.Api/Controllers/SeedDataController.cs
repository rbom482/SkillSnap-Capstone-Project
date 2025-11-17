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
                Bio = "Full-stack developer passionate about creating innovative web solutions and learning cutting-edge technologies. Specialized in .NET ecosystem with expertise in modern web development.",
                ProfileImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300&h=300&fit=crop&crop=face",
                Projects = new List<Project>
                {
                    new Project 
                    { 
                        Title = "SkillSnap Portfolio Manager", 
                        Description = "A comprehensive portfolio management system built with Blazor WebAssembly and ASP.NET Core. Features JWT authentication, performance caching, and responsive design.",
                        Technologies = "C#, Blazor WebAssembly, ASP.NET Core, Entity Framework, SQLite",
                        ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=400&h=200&fit=crop",
                        GitHubUrl = "https://github.com/demo/skillsnap",
                        LiveDemoUrl = "https://skillsnap-demo.azurewebsites.net",
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        UpdatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new Project 
                    { 
                        Title = "E-Commerce Platform", 
                        Description = "Modern e-commerce solution with real-time inventory management, secure payment processing, and advanced analytics dashboard.",
                        Technologies = "React, Node.js, MongoDB, Stripe API, Redis",
                        ImageUrl = "https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=400&h=200&fit=crop",
                        GitHubUrl = "https://github.com/demo/ecommerce-platform",
                        LiveDemoUrl = "https://ecommerce-demo.herokuapp.com",
                        CreatedAt = DateTime.UtcNow.AddDays(-60),
                        UpdatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new Project 
                    { 
                        Title = "Weather Analytics Dashboard", 
                        Description = "Interactive weather analytics platform with real-time data visualization, forecasting algorithms, and location-based insights.",
                        Technologies = "Vue.js, Python Flask, PostgreSQL, Chart.js, OpenWeather API",
                        ImageUrl = "https://images.unsplash.com/photo-1504608524841-42fe6f032b4b?w=400&h=200&fit=crop",
                        GitHubUrl = "https://github.com/demo/weather-analytics",
                        LiveDemoUrl = "https://weather-analytics.netlify.app",
                        CreatedAt = DateTime.UtcNow.AddDays(-90),
                        UpdatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new Project 
                    { 
                        Title = "Task Management API", 
                        Description = "RESTful API for team collaboration and task management with role-based permissions, real-time notifications, and comprehensive reporting.",
                        Technologies = "ASP.NET Core Web API, SignalR, Entity Framework, JWT, Swagger",
                        ImageUrl = "https://images.unsplash.com/photo-1611224923853-80b023f02d71?w=400&h=200&fit=crop",
                        GitHubUrl = "https://github.com/demo/task-management-api",
                        CreatedAt = DateTime.UtcNow.AddDays(-45),
                        UpdatedAt = DateTime.UtcNow.AddDays(-8)
                    }
                },
                Skills = new List<Skill>
                {
                    new Skill { Name = "C#", Level = "Expert" },
                    new Skill { Name = "ASP.NET Core", Level = "Expert" },
                    new Skill { Name = "Blazor WebAssembly", Level = "Advanced" },
                    new Skill { Name = "Entity Framework Core", Level = "Advanced" },
                    new Skill { Name = "JavaScript/TypeScript", Level = "Advanced" },
                    new Skill { Name = "React", Level = "Intermediate" },
                    new Skill { Name = "Vue.js", Level = "Intermediate" },
                    new Skill { Name = "SQL Server", Level = "Advanced" },
                    new Skill { Name = "Azure Cloud Services", Level = "Intermediate" },
                    new Skill { Name = "Docker", Level = "Intermediate" },
                    new Skill { Name = "Git/GitHub", Level = "Expert" },
                    new Skill { Name = "RESTful APIs", Level = "Expert" }
                }
            };

            _context.PortfolioUsers.Add(user);
            _context.SaveChanges();
            return Ok("Sample data inserted.");
        }
    }
}