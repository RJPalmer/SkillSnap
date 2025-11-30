using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SkillSnap.Shared.Models;

namespace SkillSnap_API.Data;

public class SkillSnapDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public SkillSnapDbContext(DbContextOptions<SkillSnapDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //
        // ApplicationUser ↔ PortfolioUser (1:1)
        //
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(a => a.PortfolioUser)
            .WithOne(p => p.ApplicationUser)
            .HasForeignKey<PortfolioUser>(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        //
        // PortfolioUser ↔ PortfolioUserProject (1:many)
        //
        modelBuilder.Entity<PortfolioUser>()
            .HasMany(pu => pu.PortfolioUserProjects)
            .WithOne(pup => pup.PortfolioUser)
            .HasForeignKey(pup => pup.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);

        //
        // PortfolioUser ↔ PortfolioUserSkill (1:many)
        //
        modelBuilder.Entity<PortfolioUser>()
            .HasMany(pu => pu.PortfolioUserSkills)
            .WithOne(pus => pus.PortfolioUser)
            .HasForeignKey(pus => pus.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);

        //
        // Project ↔ PortfolioUserProject (1:many)
        //
        modelBuilder.Entity<Project>()
            .HasMany(p => p.PortfolioUserProjects)
            .WithOne(pup => pup.Project)
            .HasForeignKey(pup => pup.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        //
        // Skill ↔ PortfolioUserSkill (1:many)
        //
        modelBuilder.Entity<Skill>()
            .HasMany(s => s.SkillPortfolioUsers)
            .WithOne(pus => pus.Skill)
            .HasForeignKey(pus => pus.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        //
        // Composite keys
        //
        modelBuilder.Entity<PortfolioUserSkill>()
            .HasKey(pus => new { pus.PortfolioUserId, pus.SkillId });

        modelBuilder.Entity<PortfolioUserProject>()
            .HasKey(pup => new { pup.PortfolioUserId, pup.ProjectId });
    }

    public DbSet<PortfolioUser> PortfolioUsers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<PortfolioUserProject> PortfolioUserProjects { get; set; }
    public DbSet<PortfolioUserSkill> PortfolioUserSkills { get; set; }
}