

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Controllers;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace SkillSnap_API_Test.Controllers
{
    public class ProjectControllerTests
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
        public async Task GetAll_ReturnsAllProjects()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            dbContext.Projects.AddRange(
                new Project { Id = 1, Title = "Project A", Description = "Description 1", ImageUrl = "ImageUrl 1" },
                new Project { Id = 2, Title = "Project B", Description = "Description 1", ImageUrl = "ImageURL 2" }
            );
            await dbContext.SaveChangesAsync();
            var controller = new ProjectController(dbContext);

            // Act
            var result = await controller.GetProjects();

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetById_ReturnsProject_WhenExists()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var project = new Project { Id = 1, Title = "Test Project", Description = "Description 1", ImageUrl = "ImageURL 1" };
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();
            var controller = new ProjectController(dbContext);

            // Act
            var result = await controller.GetProject(1);

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal("Test Project", result.Value.Title);
        }

        [Fact]
        public async Task Create_ReturnsCreatedProject_WhenValid()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var controller = new ProjectController(dbContext);
            var newProject = new ProjectCreateDto { Title = "New Project", Description = "New Description", ImageUrl = "ImageURL 1" };

            // Act
            var result = await controller.PostProject(newProject);
            var caaResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var pDto = Assert.IsType<ProjectDto>(caaResult.Value);

            // Assert
            Assert.NotNull(pDto);
            Assert.Equal("New Project", pDto.Title);
            Assert.Single(dbContext.Projects);
        }

        [Fact]
        public async Task AttachProjectToUser_AttachesProject_WhenValid()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var user = new PortfolioUser { Id = 1, Name = "John Doe", Bio = "Bio 1", ProfileImageUrl = "ImageURL1" };
            var project = new Project { Id = 100, Title = "Attachable Project", Description = "Description 1", ImageUrl = "ImageURL 1" };
            dbContext.PortfolioUsers.Add(user);
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();
            var controller = new ProjectController(dbContext);

            // Act
            var attachResult = await controller.AttachProjectToUser(1, 100);

            // Assert
            var updatedUser = await dbContext.PortfolioUsers
                .Include(u => u.Projects)
                .FirstOrDefaultAsync(u => u.Id == 1);

            Assert.NotNull(updatedUser);
            Assert.Contains(updatedUser.Projects, p => p.Id == 100);
        }

        [Fact]
        public async Task Delete_RemovesProject_WhenExists()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var project = new Project { Id = 1, Title = "To Delete", Description = "Description 1", ImageUrl = "ImageURL 1" };
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();
            var controller = new ProjectController(dbContext);

            // Act
            var result = await controller.DeleteProject(1);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            Assert.Empty(dbContext.Projects);
        }
    }
}