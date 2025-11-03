using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.models;

    public class SkillSnapDBContext : DbContext
    {
        public SkillSnapDBContext (DbContextOptions<SkillSnapDBContext> options)
            : base(options)
        {
        }

        public DbSet<SkillSnap_API.models.Skill> Skill { get; set; } = default!;

public DbSet<SkillSnap_API.models.PortfolioUser> PortfolioUser { get; set; } = default!;

public DbSet<SkillSnap_API.models.Project> Project { get; set; } = default!;
    }
