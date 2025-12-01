using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using SkillSnap_API.Data;
using SkillSnap_API.Controllers;
using SkillSnap_API.Services;
using SkillSnap.Shared.DTOs;
using SkillSnap.Shared.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SkillSnap.Shared.DTOs.Account;

namespace SkillSnap_API_Test.Controllers
{
    public class PortfolioUserControllerTests
    {
        private SkillSnapDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<SkillSnapDbContext>()
                .UseInMemoryDatabase($"SkillSnap_TestDB_{Guid.NewGuid()}")
                .Options;

            var db = new SkillSnapDbContext(options);
            db.Database.EnsureCreated();

            return db;
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );
        }

        private Mock<JwtTokenService> CreateJwtMock()
        {
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            // Default config returns null for any key used in tests; tests that need token generation stub the method.
            mockConfig.Setup(c => c[It.IsAny<string>()]).Returns((string?)null);
            var userManagerMock = CreateUserManagerMock();
            return new Mock<JwtTokenService>(mockConfig.Object, userManagerMock.Object);
        }

        // -----------------------------------------------------------
        // GET ALL
        // -----------------------------------------------------------
        [Fact]
        public async Task GetAll_ReturnsAllUsers()
        {
            var db = CreateDbContext();
            db.PortfolioUsers.Add(new PortfolioUser { Id = 1, Name = "User1", Bio = "Bio1", ProfileImageUrl = "" });
            db.PortfolioUsers.Add(new PortfolioUser { Id = 2, Name = "User2", Bio = "Bio2", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var result = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<PortfolioUserDto>>(ok.Value);

            Assert.Equal(2, dtos.Count());
        }

        // -----------------------------------------------------------
        // GET BY ID
        // -----------------------------------------------------------
        [Fact]
        public async Task GetById_ReturnsUser_WhenExists()
        {
            var db = CreateDbContext();

            var user = new PortfolioUser { Id = 10, Name = "TestUser", Bio = "Bio", ProfileImageUrl = "" };
            db.PortfolioUsers.Add(user);
            await db.SaveChangesAsync();

            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var result = await controller.GetById(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PortfolioUserDto>(ok.Value);

            Assert.Equal("TestUser", dto.Name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            var db = CreateDbContext();
            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var result = await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // -----------------------------------------------------------
        // GET BY NAME
        // -----------------------------------------------------------
        [Fact]
        public async Task GetByName_ReturnsUser_WhenExists()
        {
            var db = CreateDbContext();

            db.PortfolioUsers.Add(new PortfolioUser { Id = 1, Name = "Alpha", Bio = "Bio", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var result = await controller.GetByName("Alpha");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PortfolioUserDto>(ok.Value);

            Assert.Equal("Alpha", dto.Name);
        }

        [Fact]
        public async Task GetByName_ReturnsNotFound_WhenMissing()
        {
            var db = CreateDbContext();

            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var result = await controller.GetByName("DoesNotExist");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // -----------------------------------------------------------
        // CREATE
        // -----------------------------------------------------------
        [Fact]
        public async Task Create_ReturnsCreated_WhenValid()
        {
            var db = CreateDbContext();
            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var input = new PortfolioUserCreateDto
            {
                Name = "NewUser",
                Bio = "Test Bio",
                ProfileImageUrl = "img.png"
            };

            var result = await controller.Create(input);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var dto = Assert.IsType<PortfolioUserDto>(created.Value);

            Assert.Equal("NewUser", dto.Name);
            Assert.Equal(1, db.PortfolioUsers.Count());
        }

        // -----------------------------------------------------------
        // UPDATE
        // -----------------------------------------------------------
        [Fact]
        public async Task Update_ReturnsNoContent_WhenUpdated()
        {
            var db = CreateDbContext();

            db.PortfolioUsers.Add(new PortfolioUser { Id = 5, Name = "Old", Bio = "OldBio", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var input = new PortfolioUserCreateDto
            {
                Name = "Updated",
                Bio = "UpdatedBio"
            };

            var result = await controller.Update(5, input);

            Assert.IsType<NoContentResult>(result);

            var updated = await db.PortfolioUsers.FindAsync(5);
            Assert.Equal("Updated", updated.Name);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenMissing()
        {
            var db = CreateDbContext();
            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var input = new PortfolioUserCreateDto { Name = "DoesNotMatter" };

            var result = await controller.Update(999, input);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------------
        // DELETE
        // -----------------------------------------------------------
        [Fact]
        public async Task Delete_RemovesUser_WhenExists()
        {
            var db = CreateDbContext();
            db.PortfolioUsers.Add(new PortfolioUser { Id = 3, Name = "DeleteMe", Bio = "", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var result = await controller.Delete(3);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(db.PortfolioUsers);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            var db = CreateDbContext();

            var controller = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateJwtMock().Object);

            var result = await controller.Delete(123);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------------
        // GET MY PROFILE
        // -----------------------------------------------------------
        [Fact]
        public async Task GetMyProfile_ReturnsProfile_WhenClaimMatches()
        {
            var db = CreateDbContext();

            db.PortfolioUsers.Add(new PortfolioUser { Id = 55, Name = "Self", Bio = "UserBio", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var jwt = CreateJwtMock();
            var userManager = CreateUserManagerMock();

            var controller = new PortfolioUserController(db, userManager.Object, jwt.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = FakeHttpContextWithClaim("portfolioUserId", "55")
            };

            var result = await controller.GetMyProfile();

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<PortfolioUserDto>(ok.Value);

            Assert.Equal("Self", dto.Name);
        }

        // -----------------------------------------------------------
        // LINK PORTFOLIO USER
        // -----------------------------------------------------------
        [Fact]
        public async Task LinkPortfolioUser_LinksSuccessfully()
        {
            var db = CreateDbContext();

            var portfolioUser = new PortfolioUser { Id = 88, Name = "PU", Bio = "", ProfileImageUrl = "" };
            db.PortfolioUsers.Add(portfolioUser);

            var appUser = new ApplicationUser { Id = "app123", Email = "test@test.com" };
            db.Users.Add(appUser);

            await db.SaveChangesAsync();

            // Create a real JwtTokenService configured with a test signing key so we can exercise token generation.
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            mockConfig.Setup(c => c["Jwt:Key"]).Returns(new string('x', 32));
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test");
            mockConfig.Setup(c => c["Jwt:ExpiresInMinutes"]).Returns("60");

            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("app123")).ReturnsAsync(appUser);
            userManager.Setup(m => m.UpdateAsync(appUser)).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string>());

            var jwtService = new JwtTokenService(mockConfig.Object, userManager.Object);

            var controller = new PortfolioUserController(db, userManager.Object, jwtService);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = FakeHttpContextWithClaim(ClaimTypes.NameIdentifier, "app123")
            };

            var result = await controller.LinkPortfolioUser(88);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LinkPortfolioUserResponseDto>(ok.Value);

            Assert.False(string.IsNullOrWhiteSpace(response.Token));
            Assert.Equal(88, response.PortfolioUserId);
        }

        // -----------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------
        private static Microsoft.AspNetCore.Http.HttpContext FakeHttpContextWithClaim(string type, string value)
        {
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim> { new Claim(type, value) })
            );
            return context;
        }
    }
}