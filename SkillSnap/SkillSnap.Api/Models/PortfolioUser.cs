using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap.Api.Models
{
    public class PortfolioUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Bio { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ProfileImageUrl { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties with proper initialization
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}

