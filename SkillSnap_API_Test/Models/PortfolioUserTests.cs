

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using Xunit;

namespace SkillSnap_API_Test.Models
{
    public class PortfolioUserTests
    {
        private SkillSnapDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SkillSnapDbContext>()
                .UseInMemoryDatabase(databaseName: $"SkillSnap_UserModelTest_{System.Guid.NewGuid()}")
                .Options;

            var context = new SkillSnapDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void CanInstantiatePortfolioUser()
        {
            // Arrange & Act
            var user = new PortfolioUser { Name = "Sample User", Bio = "User Bio", ProfileImageUrl = "http://sample.com/image.png" };

            // Assert
            Assert.Equal("Sample User", user.Name);
            Assert.Equal("User Bio", user.Bio);
            Assert.Equal("http://sample.com/image.png", user.ProfileImageUrl);
        }

        [Fact]
        public void DefaultSkillsCollectionIsEmpty()
        {
            // Arrange & Act
            var user = new PortfolioUser(){Name = string.Empty, Bio = string.Empty, ProfileImageUrl = string.Empty};

            // Assert
            Assert.Empty(user.PortfolioUserSkills ?? new List<PortfolioUserSkill>());
        }

        [Fact]
        public async Task CanAddSkillToUser()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "Test User", Bio = "User Bio", ProfileImageUrl = "URL" };
            var skill = new Skill { Name = "C#", Level = "Advanced" };

            context.PortfolioUsers.Add(user);
            context.Skills.Add(skill);
            await context.SaveChangesAsync();

            // Act
            var userSkill = new PortfolioUserSkill { PortfolioUserId = user.Id, SkillId = skill.Id, PortfolioUser = user, Skill = skill };
            context.PortfolioUserSkills.Add(userSkill);
            await context.SaveChangesAsync();

            // Assert
            var result = await context.PortfolioUsers
                .Include(u => u.PortfolioUserSkills)
                .ThenInclude(us => us.Skill)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.Single(result.PortfolioUserSkills);
            Assert.Equal("C#", result.PortfolioUserSkills.First().Skill.Name);
        }
    }
}