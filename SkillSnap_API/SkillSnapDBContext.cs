using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;

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
            .HasMany(pu => pu.Projects)
            .WithOne()
            .HasForeignKey(p => p.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-many relationship between PortfolioUser and Skill
        modelBuilder.Entity<PortfolioUser>()
            .HasMany(pu => pu.Skills)
            .WithOne()
            .HasForeignKey(s => s.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Additional configurations can be added here
    }

    // DbSets
    public DbSet<PortfolioUser> PortfolioUsers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Skill> Skills { get; set; }
}