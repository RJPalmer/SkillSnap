using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using Xunit;

namespace SkillSnap_API_Test.Models
{
    public class PortfolioUserSkillJoinTests
    {
        private SkillSnapDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SkillSnapDbContext>()
                .UseInMemoryDatabase(databaseName: $"SkillSnap_JoinTest_{System.Guid.NewGuid()}")
                .Options;

            var context = new SkillSnapDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task CanCreatePortfolioUserSkillLink()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "Test User", Bio = "User Bio", ProfileImageUrl = "Image Url" };
            var skill = new Skill { Name = "C#", Level = "Advanced" };

            context.PortfolioUsers.Add(user);
            context.Skills.Add(skill);
            await context.SaveChangesAsync();

            // Act
            var userSkill = new PortfolioUserSkill { PortfolioUserId = user.Id, SkillId = skill.Id, PortfolioUser = user, Skill = skill };
            context.PortfolioUserSkills.Add(userSkill);
            await context.SaveChangesAsync();

            // Assert
            var result = await context.PortfolioUserSkills
                .Include(us => us.PortfolioUser)
                .Include(us => us.Skill)
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.Equal("Test User", result.PortfolioUser.Name);
            Assert.Equal("C#", result.Skill.Name);
        }

        [Fact]
        public async Task DeletingSkillRemovesJoinEntry()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "Test User", Bio = "User Bio", ProfileImageUrl = "Image Url"  };
            var skill = new Skill { Name = "Java", Level = "Intermediate" };

            context.PortfolioUsers.Add(user);
            context.Skills.Add(skill);
            await context.SaveChangesAsync();

            context.PortfolioUserSkills.Add(new PortfolioUserSkill { PortfolioUserId = user.Id, SkillId = skill.Id, PortfolioUser = user, Skill = skill  });
            await context.SaveChangesAsync();

            // Act
            context.Skills.Remove(skill);
            await context.SaveChangesAsync();

            // Assert
            Assert.Empty(context.PortfolioUserSkills);
            Assert.Empty(context.Skills);
        }

        [Fact]
        public async Task RemovingUserRemovesJoinEntries()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "User to Delete", Bio = "User Bio", ProfileImageUrl = "Image Url"  };
            var skill1 = new Skill { Name = "Python", Level = "Beginner" };
            var skill2 = new Skill { Name = "SQL", Level = "Advanced" };

            context.PortfolioUsers.Add(user);
            context.Skills.AddRange(skill1, skill2);
            await context.SaveChangesAsync();

            context.PortfolioUserSkills.AddRange(
                new PortfolioUserSkill { PortfolioUserId = user.Id, SkillId = skill1.Id, PortfolioUser = user, Skill = skill1 },
                new PortfolioUserSkill { PortfolioUserId = user.Id, SkillId = skill2.Id, PortfolioUser = user, Skill = skill2}
            );
            await context.SaveChangesAsync();

            // Act
            context.PortfolioUsers.Remove(user);
            await context.SaveChangesAsync();

            // Assert
            Assert.Empty(context.PortfolioUserSkills);
            Assert.Empty(context.PortfolioUsers);
        }
    }
}
