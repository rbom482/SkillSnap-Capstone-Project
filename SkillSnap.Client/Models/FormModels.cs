using System.ComponentModel.DataAnnotations;

namespace SkillSnap.Client.Models
{
    /// <summary>
    /// Model for project forms with comprehensive validation attributes
    /// Following Microsoft Copilot best practices for form validation
    /// </summary>
    public class ProjectFormModel
    {
        [Required(ErrorMessage = "Project title is required.")]
        [StringLength(200, ErrorMessage = "Project title must be 200 characters or less.")]
        [Display(Name = "Project Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Project description is required.")]
        [StringLength(1000, ErrorMessage = "Project description must be 1000 characters or less.")]
        [MinLength(10, ErrorMessage = "Project description must be at least 10 characters.")]
        [Display(Name = "Project Description")]
        public string Description { get; set; } = string.Empty;

        [Url(ErrorMessage = "Please enter a valid URL.")]
        [StringLength(500, ErrorMessage = "Image URL must be 500 characters or less.")]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Url(ErrorMessage = "Please enter a valid GitHub URL.")]
        [StringLength(500, ErrorMessage = "GitHub URL must be 500 characters or less.")]
        [Display(Name = "GitHub Repository URL")]
        public string? GitHubUrl { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL.")]
        [StringLength(500, ErrorMessage = "Live demo URL must be 500 characters or less.")]
        [Display(Name = "Live Demo URL")]
        public string? LiveDemoUrl { get; set; }

        [StringLength(200, ErrorMessage = "Technologies must be 200 characters or less.")]
        [Display(Name = "Technologies Used")]
        public string? Technologies { get; set; }

        [Required(ErrorMessage = "Portfolio User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Portfolio User ID.")]
        public int PortfolioUserId { get; set; } = 1;

        /// <summary>
        /// Converts the form model to a Project entity
        /// </summary>
        /// <returns>Project entity</returns>
        public Services.Project ToProject()
        {
            return new Services.Project
            {
                Title = Title,
                Description = Description,
                ImageUrl = ImageUrl ?? string.Empty,
                GitHubUrl = GitHubUrl,
                LiveDemoUrl = LiveDemoUrl,
                Technologies = Technologies,
                PortfolioUserId = PortfolioUserId
            };
        }

        /// <summary>
        /// Creates a form model from an existing project
        /// </summary>
        /// <param name="project">Existing project</param>
        /// <returns>Project form model</returns>
        public static ProjectFormModel FromProject(Services.Project project)
        {
            return new ProjectFormModel
            {
                Title = project.Title,
                Description = project.Description,
                ImageUrl = project.ImageUrl,
                GitHubUrl = project.GitHubUrl,
                LiveDemoUrl = project.LiveDemoUrl,
                Technologies = project.Technologies,
                PortfolioUserId = project.PortfolioUserId
            };
        }

        /// <summary>
        /// Resets the form to its default state
        /// </summary>
        public void Reset()
        {
            Title = string.Empty;
            Description = string.Empty;
            ImageUrl = null;
            GitHubUrl = null;
            LiveDemoUrl = null;
            Technologies = null;
            PortfolioUserId = 1;
        }
    }

    /// <summary>
    /// Model for skill forms with comprehensive validation attributes
    /// </summary>
    public class SkillFormModel
    {
        [Required(ErrorMessage = "Skill name is required.")]
        [StringLength(100, ErrorMessage = "Skill name must be 100 characters or less.")]
        [MinLength(2, ErrorMessage = "Skill name must be at least 2 characters.")]
        [Display(Name = "Skill Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Skill level is required.")]
        [Display(Name = "Proficiency Level")]
        public string Level { get; set; } = string.Empty;

        [Required(ErrorMessage = "Portfolio User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Portfolio User ID.")]
        public int PortfolioUserId { get; set; } = 1;

        /// <summary>
        /// Gets the available skill levels
        /// </summary>
        public static List<string> SkillLevels => new()
        {
            "Beginner",
            "Intermediate", 
            "Advanced",
            "Expert"
        };

        /// <summary>
        /// Converts the form model to a Skill entity
        /// </summary>
        /// <returns>Skill entity</returns>
        public Services.Skill ToSkill()
        {
            return new Services.Skill
            {
                Name = Name,
                Level = Level,
                PortfolioUserId = PortfolioUserId
            };
        }

        /// <summary>
        /// Creates a form model from an existing skill
        /// </summary>
        /// <param name="skill">Existing skill</param>
        /// <returns>Skill form model</returns>
        public static SkillFormModel FromSkill(Services.Skill skill)
        {
            return new SkillFormModel
            {
                Name = skill.Name,
                Level = skill.Level,
                PortfolioUserId = skill.PortfolioUserId
            };
        }

        /// <summary>
        /// Resets the form to its default state
        /// </summary>
        public void Reset()
        {
            Name = string.Empty;
            Level = string.Empty;
            PortfolioUserId = 1;
        }
    }
}