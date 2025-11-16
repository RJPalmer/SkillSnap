using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SkillSnap_API.Data;

namespace SkillSnap_API.Data;

public class SkillSnapDbContextFactory : IDesignTimeDbContextFactory<SkillSnapDbContext>
{
    public SkillSnapDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SkillSnapDbContext>();
        optionsBuilder.UseSqlite("Data Source=skillsnap.db");

        return new SkillSnapDbContext(optionsBuilder.Options);
    }
}