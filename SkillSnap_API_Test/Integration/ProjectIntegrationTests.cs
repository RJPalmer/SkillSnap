

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Controllers;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using Xunit;
using SkillSnap.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SkillSnap_API.Services;
using Microsoft.Extensions.Logging;

namespace SkillSnap_API_Test.Integration
{
    public class ProjectIntegrationTests
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
        public async Task GetProjects_ReturnsProjects()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            dbContext.Projects.AddRange(
                new Project { Id = 1, Title = "Test Project 1", Description = "Description 1", ImageUrl = "ImageURL 1" },
                new Project { Id = 2, Title = "Test Project 2", Description = "Description 2", ImageUrl = "ImageURL 2" }
            );
            await dbContext.SaveChangesAsync();

            var cacheService = new SkillSnap_API_Test.Utils.TestCacheService();
            var mockLogger = new Mock<ILogger<ProjectController>>();
            var controller = new ProjectController(dbContext, cacheService, mockLogger.Object);

            // Act
            var result = await controller.GetProjects();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var projects = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(ok.Value);
            Assert.Equal(2, projects.Count());
        }

        [Fact]
        public async Task AttachProjectToUser_AddsProject()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var user = new PortfolioUser { Id = 99, Name = "Tester", Bio = "Bio 1", ProfileImageUrl = "Image Url 1" };
            var project = new Project { Id = 100, Title = "Attachable Project", Description = "Description 1", ImageUrl = "ImageURL 1" };

            dbContext.PortfolioUsers.Add(user);
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var cacheService2 = new SkillSnap_API_Test.Utils.TestCacheService();
            var mockLogger2 = new Mock<ILogger<ProjectController>>();
            var controller = new ProjectController(dbContext, cacheService2, mockLogger2.Object);

            var joinEntry = new PortfolioUserProjectCreateDto()
            {
                PortfolioUserId = 99,
                ProjectId = 100
            };
            // Act
            var attachResult = await controller.AttachProjectToUser(joinEntry);

            // Assert
            var updatedUser = await dbContext.PortfolioUsers
                .Include(u => u.PortfolioUserProjects).ThenInclude(pup => pup.Project)
                .FirstOrDefaultAsync(u => u.Id == 99);

            Assert.NotNull(updatedUser);
            Assert.Contains(updatedUser.PortfolioUserProjects, p => p.ProjectId == 100);
        }
    }
}