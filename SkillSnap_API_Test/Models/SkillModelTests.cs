using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using Xunit;

namespace SkillSnap_API_Test.Models
{
    public class SkillModelTests
    {
        private SkillSnapDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SkillSnapDbContext>()
                .UseInMemoryDatabase(databaseName: $"SkillSnap_SkillModelTest_{System.Guid.NewGuid()}")
                .Options;

            var context = new SkillSnapDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void CanInstantiateSkill()
        {
            // Arrange & Act
            var skill = new Skill { Name = "JavaScript", Level = "Intermediate" };

            // Assert
            Assert.Equal("JavaScript", skill.Name);
            Assert.Equal("Intermediate", skill.Level);
        }

        [Fact]
        public async Task CanAddSkillToDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var skill = new Skill { Name = "C#", Level = "Advanced" };

            // Act
            context.Skills.Add(skill);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, await context.Skills.CountAsync());
            Assert.Equal("C#", (await context.Skills.FirstAsync()).Name);
        }

        [Fact]
        public async Task DefaultPortfolioUserSkillsCollectionIsEmpty()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var skill = new Skill { Name = "Python", Level = "Beginner" };

            context.Skills.Add(skill);
            await context.SaveChangesAsync();

            // Act
            var result = await context.Skills.Include(s => s.SkillPortfolioUsers).FirstOrDefaultAsync();

            // Assert
            Assert.Empty(result.SkillPortfolioUsers ?? new System.Collections.Generic.List<PortfolioUserSkill>());
        }
    }
}