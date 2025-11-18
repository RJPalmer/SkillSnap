

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Controllers;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using Xunit;

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

            var controller = new ProjectController(dbContext);

            // Act
            var result = await controller.GetProjects();

            // Assert
            Assert.NotNull(result);
            var projects = result.Value;
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

            var controller = new ProjectController(dbContext);

            // Act
            var attachResult = await controller.AttachProjectToUser(99, 100);

            // Assert
            var updatedUser = await dbContext.PortfolioUsers
                .Include(u => u.portfolioUserProjects).ThenInclude(pup => pup.Project)
                .FirstOrDefaultAsync(u => u.Id == 99);

            Assert.NotNull(updatedUser);
            Assert.Contains(updatedUser.portfolioUserProjects, p => p.ProjectId == 100);
        }
    }
}