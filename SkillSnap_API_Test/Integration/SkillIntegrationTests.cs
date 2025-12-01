using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Controllers;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SkillSnap_API.Services;
using Microsoft.Extensions.Logging;

namespace SkillSnap_API_Test.Integration
{
    public class SkillIntegrationTests
    {
        private SkillSnapDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SkillSnapDbContext>()
                .UseInMemoryDatabase(databaseName: $"SkillSnap_TestDB_{System.Guid.NewGuid()}")
                .Options;

            var context = new SkillSnapDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnsAllSkills()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Skills.AddRange(
                new Skill { Name = "C#", Level = "Advanced" },
                new Skill { Name = "SQL", Level = "Intermediate" }
            );
            await context.SaveChangesAsync();
                var cacheService = new SkillSnap_API_Test.Utils.TestCacheService();
            var mockLogger = new Mock<ILogger<SkillController>>();
            var controller = new SkillController(context, cacheService, mockLogger.Object);

            // Act
            var result = await controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var sDto = Assert.IsAssignableFrom<IEnumerable<SkillDto>>(okResult.Value);

            // Assert
            Assert.NotNull(sDto);
            Assert.Equal(2, sDto.Count());
        }

        [Fact]
        public async Task Create_AddsNewSkill()
        {
            // Arrange
            var context = GetInMemoryDbContext();
                var cacheService2 = new SkillSnap_API_Test.Utils.TestCacheService();
            var mockLogger2 = new Mock<ILogger<SkillController>>();
            var controller = new SkillController(context, cacheService2, mockLogger2.Object);
            var newSkill = new SkillCreateDto { Name = "Python", Level = "Beginner" };

            // Act
            var result = await controller.Create(newSkill);

            // Assert
            var createdSkill = context.Skills.FirstOrDefault();
            Assert.NotNull(createdSkill);
            Assert.Equal("Python", createdSkill.Name);
            Assert.Equal("Beginner", createdSkill.Level);
        }

        [Fact]
        public async Task Delete_RemovesSkill_WhenExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var skill = new Skill { Name = "ToDelete", Level = "Advanced" };
            context.Skills.Add(skill);
            await context.SaveChangesAsync();
                var cacheService3 = new SkillSnap_API_Test.Utils.TestCacheService();
            var mockLogger3 = new Mock<ILogger<SkillController>>();
            var controller = new SkillController(context, cacheService3, mockLogger3.Object);

            // Act
            var result = await controller.Delete(skill.Id);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            Assert.Empty(context.Skills);
        }
    }
}