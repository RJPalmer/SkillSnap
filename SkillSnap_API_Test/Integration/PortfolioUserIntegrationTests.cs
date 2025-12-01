using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using SkillSnap_API.Controllers;
using SkillSnap_API.Data;
using SkillSnap_API.Services;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

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

        ///<summary>
        /// Mocks the UserManager for testing purposes.
        /// </summary>
        private UserManager<ApplicationUser> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mockOptions = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new IdentityOptions());

            var mockPasswordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var mockUserValidators = new[] { new Mock<IUserValidator<ApplicationUser>>().Object };
            var mockPasswordValidators = new[] { new Mock<IPasswordValidator<ApplicationUser>>().Object };
            var mockKeyNormalizer = new Mock<ILookupNormalizer>();
            var mockErrors = new IdentityErrorDescriber();
            var mockLogger = new Mock<ILogger<UserManager<ApplicationUser>>>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            return new UserManager<ApplicationUser>(
                store.Object,
                mockOptions.Object,              // FIXED: not null
                mockPasswordHasher.Object,
                mockUserValidators,
                mockPasswordValidators,
                mockKeyNormalizer.Object,
                mockErrors,
                mockServiceProvider.Object,      // FIXED: not null
                mockLogger.Object
            );
        }

        private JwtTokenService MockJwtTokenService()
        {
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            // Provide real values – not null
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("THIS_IS_A_TEST_KEY_32_CHARS_MINIMUM_1234");
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            mockConfig.Setup(c => c["Jwt:ExpiresInMinutes"]).Returns("60");
            var userManagerMock = MockUserManager();
            var jwt = new JwtTokenService(mockConfig.Object, userManagerMock);
            return jwt;
        }

        // ------------------------------------------------------------
        // GET ALL
        // ------------------------------------------------------------
        [Fact]
        public async Task GetAll_ReturnsAllUsers()
        {
            var context = GetInMemoryDbContext();
            context.PortfolioUsers.AddRange(
                new PortfolioUser { Name = "User A", Bio = "Bio A", ProfileImageUrl = "Url A" },
                new PortfolioUser { Name = "User B", Bio = "Bio B", ProfileImageUrl = "Url B" }
            );
            await context.SaveChangesAsync();

            var controller = new PortfolioUserController(context, MockUserManager(), MockJwtTokenService());

            var result = await controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var puDto = Assert.IsAssignableFrom<IEnumerable<PortfolioUserDto>>(okResult.Value);

            Assert.NotNull(puDto);
            Assert.Equal(2, puDto.Count());
        }

        // ------------------------------------------------------------
        // GET BY ID
        // ------------------------------------------------------------
        [Fact]
        public async Task GetById_ReturnsUser_WhenExists()
        {
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "Test User", Bio = "Test Bio", ProfileImageUrl = "Image URL" };
            context.PortfolioUsers.Add(user);
            await context.SaveChangesAsync();

            var controller = new PortfolioUserController(context, MockUserManager(), MockJwtTokenService());

            var result = await controller.GetById(user.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PortfolioUserDto>(okResult.Value);
            Assert.Equal("Test User", dto.Name);
        }

        [Fact]
        public async Task Create_AddsNewUser()
        {
            var context = GetInMemoryDbContext();
            var controller = new PortfolioUserController(context, MockUserManager(), MockJwtTokenService());
            var newUser = new PortfolioUserCreateDto { Name = "New User", Bio = "New Bio", ProfileImageUrl = "Image URL" };

            var result = await controller.Create(newUser);

            var createdUser = context.PortfolioUsers.FirstOrDefault();
            Assert.NotNull(createdUser);
            Assert.Equal("New User", createdUser.Name);
        }

        [Fact]
        public async Task Delete_RemovesUser_WhenExists()
        {
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "User To Delete", Bio = "Bio", ProfileImageUrl = "Image URL" };
            context.PortfolioUsers.Add(user);
            await context.SaveChangesAsync();
            var controller = new PortfolioUserController(context, MockUserManager(), MockJwtTokenService());

            var result = await controller.Delete(user.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.PortfolioUsers);
        }

        [Fact]
        public async Task UpdateUserSkills_AddsNewSkills()
        {
            var context = GetInMemoryDbContext();
            var user = new PortfolioUser { Name = "User With Skills", Bio = "User Bio", ProfileImageUrl = "ImageURL 1" };
            var skill = new Skill { Name = "C#", Level = "Beginner" };
            context.PortfolioUsers.Add(user);
            context.Skills.Add(skill);
            await context.SaveChangesAsync();
            var controller = new PortfolioUserController(context, MockUserManager(), MockJwtTokenService());

            var updatedSkills = new[] { "C#", "SQL" }; // SQL does not exist yet
            var result = await controller.UpdateUserSkills(user.Id, updatedSkills);

            var updatedUser = await context.PortfolioUsers
                .Include(u => u.PortfolioUserSkills)
                .ThenInclude(pus => pus.Skill)
                .FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal(2, updatedUser.PortfolioUserSkills.Count);
            Assert.Contains(updatedUser.PortfolioUserSkills, s => string.Equals(s.Skill.Name, "SQL", System.StringComparison.OrdinalIgnoreCase));
            Assert.Contains(updatedUser.PortfolioUserSkills, s => string.Equals(s.Skill.Name, "C#", System.StringComparison.OrdinalIgnoreCase));
        }
    }
}