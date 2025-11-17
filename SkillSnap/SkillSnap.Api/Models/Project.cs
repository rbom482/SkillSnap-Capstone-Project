using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap.Api.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? GitHubUrl { get; set; }

        [MaxLength(500)]
        public string? LiveDemoUrl { get; set; }

        [MaxLength(200)]
        public string? Technologies { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key with proper configuration
        [Required]
        public int PortfolioUserId { get; set; }

        // Navigation property with virtual for lazy loading
        [ForeignKey(nameof(PortfolioUserId))]
        public virtual PortfolioUser PortfolioUser { get; set; } = null!;
    }
}
