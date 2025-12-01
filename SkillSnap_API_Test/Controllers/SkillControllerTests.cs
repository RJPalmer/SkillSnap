using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Controllers;
using SkillSnap_API.Data;
using SkillSnap_API.Services;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging;

namespace SkillSnap_API_Test.Controllers;

public class SkillControllerTests
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

        private SkillController CreateController(SkillSnapDbContext context)
        {
            var cacheService = new SkillSnap_API_Test.Utils.TestCacheService();
            var mockLogger = new Mock<ILogger<SkillController>>();
            return new SkillController(context, cacheService, mockLogger.Object);
        }

    [Fact]
    public async Task GetAll_ReturnsListOfSkills()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.Skills.AddRange(
            new Skill { Id = 1, Name = "C#", Level = "Beginner" },
            new Skill { Id = 2, Name = "JavaScript", Level = "Beginner" }
        );
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        // Act
        var result = await controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var skillDtoItem = Assert.IsAssignableFrom<IEnumerable<SkillDto>>(okResult.Value);

        // Assert
        Assert.NotNull(result);
        var skills = skillDtoItem.ToList();
        Assert.Equal(2, skills.Count);
        Assert.Contains(skills, s => s.Name == "C#");
        Assert.Contains(skills, s => s.Name == "JavaScript");
    }

    [Fact]
    public async Task Create_ReturnsCreatedSkill()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var controller = CreateController(context);
        var newSkillDto = new SkillCreateDto { Name = "Python", Level = "Beginner" };

        // Act
        var createdSkill = await controller.Create(newSkillDto);
        var createdResult = Assert.IsType<CreatedAtActionResult>(createdSkill.Result);
        var skillDtoItem = Assert.IsType<SkillDto>(createdResult.Value);
        
        // Assert
        Assert.NotNull(skillDtoItem);
        Assert.Equal("Python", skillDtoItem.Name);
        Assert.True(skillDtoItem.Id > 0);

        var skillInDb = await context.Skills.FindAsync(skillDtoItem.Id);
        Assert.NotNull(skillInDb);
        Assert.Equal("Python", skillInDb.Name);
    }

    [Fact]
    public async Task Delete_RemovesSkill_AndJoinEntries()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var skill = new Skill { Name = "Go", Level = "Beginner" };
        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        var user = new PortfolioUser { Name = "testuser", Bio = "New Bio", ProfileImageUrl = "" };
        context.PortfolioUsers.Add(user);
        await context.SaveChangesAsync();

        var userSkill = new PortfolioUserSkill { PortfolioUserId = user.Id, SkillId = skill.Id, PortfolioUser = user, Skill = skill };
        context.PortfolioUserSkills.Add(userSkill);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        // Act
        await controller.Delete(skill.Id);

        // Assert
        var skillInDb = await context.Skills.FindAsync(skill.Id);
        Assert.Null(skillInDb);

        var userSkillInDb = await context.PortfolioUserSkills
            .FirstOrDefaultAsync(us => us.SkillId == skill.Id && us.PortfolioUserId == user.Id);
        Assert.Null(userSkillInDb);
    }
}
