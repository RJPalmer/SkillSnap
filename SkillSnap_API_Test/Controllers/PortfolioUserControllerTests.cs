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
    public class PortfolioUserControllerTests
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
        public async Task GetById_ReturnsUser_WhenExists()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var user = new PortfolioUser { Id = 1, Name = "Test User", Bio = "Test Bio", ProfileImageUrl = "ImageURL 1" };
            dbContext.PortfolioUsers.Add(user);
            await dbContext.SaveChangesAsync();
            var controller = new PortfolioUserController(dbContext);

            // Act
            var result = await controller.GetById(1);
            var okresult = Assert.IsType<OkObjectResult>(result.Result);
            var puDto = Assert.IsType<PortfolioUserDto>(okresult.Value);
            

            // Assert
            Assert.NotNull(puDto);
            Assert.Equal("Test User", puDto.Name);
        }

        [Fact]
        public async Task Create_ReturnsCreatedResult_WhenValid()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var controller = new PortfolioUserController(dbContext);
            var newUser = new PortfolioUserCreateDto { Name = "New User", Bio = "New Bio" };

            // Act
            var result = await controller.Create(newUser);
            var okresult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var puDto = Assert.IsType<PortfolioUserDto>(okresult.Value);
            // Assert
            Assert.NotNull(puDto);
            Assert.Equal("New User", puDto.Name);
            Assert.Equal(1, dbContext.PortfolioUsers.Count());
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenUserUpdated()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var user = new PortfolioUser { Id = 1, Name = "Old Name", Bio = "Old Bio" , ProfileImageUrl = "ImageURL 1"};
            dbContext.PortfolioUsers.Add(user);
            await dbContext.SaveChangesAsync();
            var controller = new PortfolioUserController(dbContext);
            var updatedUser = new PortfolioUserCreateDto { Name = "Updated Name", Bio = "Updated Bio" };

            // Act
            var result = await controller.Update(1, updatedUser);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var updated = await dbContext.PortfolioUsers.FindAsync(1);
            Assert.Equal("Updated Name", updated.Name);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var controller = new PortfolioUserController(dbContext);
            var updatedUser = new PortfolioUserCreateDto { Name = "Updated Name", Bio = "Updated Bio" };

            // Act
            var result = await controller.Update(999, updatedUser);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_RemovesUser_WhenExists()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var user = new PortfolioUser { Id = 1, Name = "To Delete", Bio = "Bio", ProfileImageUrl = "ImageURL 1" };
            dbContext.PortfolioUsers.Add(user);
            await dbContext.SaveChangesAsync();
            var controller = new PortfolioUserController(dbContext);

            // Act
            var result = await controller.Delete(1);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            Assert.Empty(dbContext.PortfolioUsers);
        }
    }
}