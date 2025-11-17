using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Data
{
    public class SkillSnapContext : DbContext
    {
        public SkillSnapContext(DbContextOptions<SkillSnapContext> options) : base(options) { }
        
        public DbSet<PortfolioUser> PortfolioUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Skill> Skills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PortfolioUser entity
            modelBuilder.Entity<PortfolioUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");

                // Configure index for better query performance
                entity.HasIndex(e => e.Name);
            });

            // Configure Project entity with one-to-many relationship
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.GitHubUrl).HasMaxLength(500);
                entity.Property(e => e.LiveDemoUrl).HasMaxLength(500);
                entity.Property(e => e.Technologies).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");

                // Configure one-to-many relationship
                entity.HasOne(p => p.PortfolioUser)
                      .WithMany(u => u.Projects)
                      .HasForeignKey(p => p.PortfolioUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure indexes
                entity.HasIndex(e => e.PortfolioUserId);
                entity.HasIndex(e => e.Title);
            });

            // Configure Skill entity with one-to-many relationship
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Level).IsRequired().HasMaxLength(50);

                // Configure one-to-many relationship
                entity.HasOne(s => s.PortfolioUser)
                      .WithMany(u => u.Skills)
                      .HasForeignKey(s => s.PortfolioUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure indexes
                entity.HasIndex(e => e.PortfolioUserId);
                entity.HasIndex(e => new { e.Name, e.Level });
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is PortfolioUser || x.Entity is Project)
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    if (entity.Entity is PortfolioUser user)
                    {
                        user.CreatedAt = DateTime.UtcNow;
                        user.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entity.Entity is Project project)
                    {
                        project.CreatedAt = DateTime.UtcNow;
                        project.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else if (entity.State == EntityState.Modified)
                {
                    if (entity.Entity is PortfolioUser user)
                    {
                        user.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entity.Entity is Project project)
                    {
                        project.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}