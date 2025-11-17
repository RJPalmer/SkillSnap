using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;

namespace SkillSnap_API.Data;

public class SkillSnapDbContext : DbContext
{
    // Constructor
    public SkillSnapDbContext(DbContextOptions<SkillSnapDbContext> options) : base(options)
    {
    }

    // Model configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure one-to-many relationship between PortfolioUser and Project
        modelBuilder.Entity<PortfolioUser>()
            .HasMany(pu => pu.portfolioUserProjects)
            .WithOne()
            .HasForeignKey(pup => pup.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PortfolioUser>()
            .HasMany(pu => pu.PortfolioUserSkills)
            .WithOne()
            .HasForeignKey(pus => pus.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Project>()
            .HasMany(pup => pup.portfolioUserProjects)
            .WithOne()
            .HasForeignKey(p => p.projectId);
        modelBuilder.Entity<Skill>()
            .HasMany(s => s.SkillPortfolioUsers)
            .WithOne()
            .HasForeignKey(pus => pus.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PortfolioUserSkill>()
            .HasKey(pus => new { pus.PortfolioUserId, pus.SkillId });
        modelBuilder.Entity<PortfolioUserProject>()
            .HasKey(pup => new {pup.PortfolioUserId, pup.projectId});
            
       
        
        // Additional configurations can be added here
    }

    // DbSets
    public DbSet<PortfolioUser> PortfolioUsers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<PortfolioUserProject> PortfolioUserProjects{get; set;}       
    public DbSet<PortfolioUserSkill> PortfolioUserSkills{ get; set; }
}