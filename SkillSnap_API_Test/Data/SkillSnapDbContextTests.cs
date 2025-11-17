

using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using Xunit;
using System.Threading.Tasks;

namespace SkillSnap_API_Test.Data
{
    public class SkillSnapDbContextTests
    {
        private SkillSnapDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SkillSnapDbContext>()
                .UseInMemoryDatabase(databaseName: $"SkillSnap_DbContextTest_{System.Guid.NewGuid()}")
                .Options;

            var context = new SkillSnapDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task CanInsertPortfolioUserIntoDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "Test User", Bio = "Bio Sample", ProfileImageUrl = "http://test" };

            // Act
            context.PortfolioUsers.Add(user);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, await context.PortfolioUsers.CountAsync());
            Assert.Equal("Test User", (await context.PortfolioUsers.FirstAsync()).Name);
        }

        [Fact]
        public async Task CanInsertSkillAndUserSkillJoin()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "User1", Bio = "User Bio", ProfileImageUrl = "Image Url1" };
            var skill = new Skill { Name = "C#", Level = "Advanced" };

            context.PortfolioUsers.Add(user);
            context.Skills.Add(skill);
            await context.SaveChangesAsync();

            var userSkill = new PortfolioUserSkill { PortfolioUserId = user.Id, SkillId = skill.Id, PortfolioUser = user, Skill
            = skill };
            context.PortfolioUserSkills.Add(userSkill);
            await context.SaveChangesAsync();

            // Act
            var result = await context.PortfolioUserSkills
                .Include(pus => pus.PortfolioUser)
                .Include(pus => pus.Skill)
                .FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("User1", result.PortfolioUser.Name);
            Assert.Equal("C#", result.Skill.Name);
        }
    }
}