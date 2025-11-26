using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SkillSnap.Shared.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SQLitePCL;

namespace SkillSnap_API.Data;

public class SkillSnapDbContext : IdentityDbContext<ApplicationUser>
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
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(a => a.PortfolioUser)
            .WithOne(p => p.ApplicationUser)
            .HasForeignKey<PortfolioUser>(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Project>()
            .HasMany(pup => pup.portfolioUserProjects)
            .WithOne()
            .HasForeignKey(p => p.ProjectId);
        modelBuilder.Entity<Skill>()
            .HasMany(s => s.SkillPortfolioUsers)
            .WithOne()
            .HasForeignKey(pus => pus.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PortfolioUserSkill>()
            .HasKey(pus => new { pus.PortfolioUserId, pus.SkillId });
        modelBuilder.Entity<PortfolioUserProject>()
            .HasKey(pup => new {pup.PortfolioUserId, pup.ProjectId});
            
       
        
        // Additional configurations can be added here
    }

    // DbSets
    public DbSet<PortfolioUser> PortfolioUsers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<PortfolioUserProject> PortfolioUserProjects{get; set;}       
    public DbSet<PortfolioUserSkill> PortfolioUserSkills{ get; set; }
}