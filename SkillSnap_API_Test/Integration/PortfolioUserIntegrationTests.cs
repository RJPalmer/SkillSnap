

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Controllers;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace SkillSnap_API_Test.Integration
{
    public class PortfolioUserIntegrationTests
    {
        private SkillSnapDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SkillSnapDbContext>()
                .UseInMemoryDatabase(databaseName: $"SkillSnap_IntegrationTest_{System.Guid.NewGuid()}")
                .Options;

            var context = new SkillSnapDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnsAllUsers()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.PortfolioUsers.AddRange(
                new PortfolioUser { Name = "User A", Bio = "Bio A", ProfileImageUrl = "Url A" },
                new PortfolioUser { Name = "User B", Bio = "Bio B", ProfileImageUrl = "Url B" }
            );
            await context.SaveChangesAsync();
            var controller = new PortfolioUserController(context);

            // Act
            var result = await controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var puDto = Assert.IsAssignableFrom<IEnumerable<PortfolioUserDto>>(okResult.Value);

            // Assert
            Assert.NotNull(puDto);
            Assert.Equal(2, puDto.Count());
        }

        [Fact]
        public async Task GetById_ReturnsUser_WhenExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "Test User", Bio = "Test Bio", ProfileImageUrl = "Image URL" };
            context.PortfolioUsers.Add(user);
            await context.SaveChangesAsync();

            var controller = new PortfolioUserController(context);

            // Act
            var result = await controller.GetById(user.Id);

            // Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var dto = Assert.IsType<PortfolioUserDto>(okResult.Value);
            Assert.Equal("Test User", dto.Name);
        }

        [Fact]
        public async Task Create_AddsNewUser()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new PortfolioUserController(context);
            var newUser = new PortfolioUserCreateDto { Name = "New User", Bio = "New Bio", ProfileImageUrl = "Image URL" };

            // Act
            var result = await controller.Create(newUser);

            // Assert
            var createdUser = context.PortfolioUsers.FirstOrDefault();
            Assert.NotNull(createdUser);
            Assert.Equal("New User", createdUser.Name);
        }

        [Fact]
        public async Task Delete_RemovesUser_WhenExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "User To Delete", Bio = "Bio", ProfileImageUrl = "Image URL" };
            context.PortfolioUsers.Add(user);
            await context.SaveChangesAsync();
            var controller = new PortfolioUserController(context);

            // Act
            var result = await controller.Delete(user.Id);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            Assert.Empty(context.PortfolioUsers);
        }

        [Fact]
        public async Task UpdateUserSkills_AddsNewSkills()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "User With Skills", Bio = "User Bio", ProfileImageUrl ="ImageURL 1" };
            var skill = new Skill { Name = "C#", Level = "Beginner" };
            context.PortfolioUsers.Add(user);
            context.Skills.Add(skill);
            await context.SaveChangesAsync();
            var controller = new PortfolioUserController(context);

            // Act
            var updatedSkills = new[] { "C#", "SQL" }; // SQL does not exist yet
            var result = await controller.UpdateUserSkills(user.Id, updatedSkills);

            // Assert
            var updatedUser = await context.PortfolioUsers
                .Include(u => u.PortfolioUserSkills)
                .ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal(2, updatedUser.PortfolioUserSkills.Count);
            Assert.Contains(updatedUser.PortfolioUserSkills, s => s.Skill.Name == "SQL".ToLower());
            Assert.Contains(updatedUser.PortfolioUserSkills, s => s.Skill.Name == "C#");
        }
    }
}