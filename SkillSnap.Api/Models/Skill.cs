using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap.Api.Models
{           
    public class Skill  
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Level { get; set; }
       

        // Foreign key
        public int PortfolioUserId { get; set; }

        // Navigation property
        [ForeignKey("PortfolioUserId")]
        public PortfolioUser PortfolioUser { get; set; }
    }
}