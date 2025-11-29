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
            return new Mock<JwtTokenService>(null, null);
        }

        // -----------------------------------------------------------
        // GET ALL
        // -----------------------------------------------------------
        [Fact]
        public async Task GetAll_ReturnsAllUsers()
        {
            var db = CreateDbContext();
            db.PortfolioUsers.Add(new /* `PortfolioUser` seems to be a model class representing a user's portfolio information. It contains properties such as `Id`, `Name`, `Bio`, and possibly `ProfileImageUrl`. The tests in the `PortfolioUserControllerTests` class are testing various functionalities related to managing and interacting with `PortfolioUser` data, such as retrieving all users, getting a user by ID or name, creating a new user, updating user information, deleting a user, and linking a portfolio user to an application user. */
            PortfolioUser { Id = 1, Name = "User1", Bio = "Bio1" });
            db.PortfolioUsers.Add(new PortfolioUser { Id = 2, Name = "User2", Bio = "Bio2" });
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

            var user = new PortfolioUser { Id = 10, Name = "TestUser", Bio = "Bio" };
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

            db.PortfolioUsers.Add(new PortfolioUser { Id = 1, Name = "Alpha", Bio = "Bio" });
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

            db.PortfolioUsers.Add(new PortfolioUser { Id = 5, Name = "Old", Bio = "OldBio" });
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
            db.PortfolioUsers.Add(new PortfolioUser { Id = 3, Name = "DeleteMe" });
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

            db.PortfolioUsers.Add(new PortfolioUser { Id = 55, Name = "Self", Bio = "UserBio" });
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

            var portfolioUser = new PortfolioUser { Id = 88, Name = "PU" };
            db.PortfolioUsers.Add(portfolioUser);

            var appUser = new ApplicationUser { Id = "app123", Email = "test@test.com" };
            db.Users.Add(appUser);

            await db.SaveChangesAsync();

            var jwt = CreateJwtMock();
            jwt.Setup(j => j.GenerateToken(It.IsAny<ApplicationUser>())).Returns("NEW_TOKEN");

            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("app123")).ReturnsAsync(appUser);
            userManager.Setup(m => m.UpdateAsync(appUser)).ReturnsAsync(IdentityResult.Success);

            var controller = new PortfolioUserController(db, userManager.Object, jwt.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = FakeHttpContextWithClaim(ClaimTypes.NameIdentifier, "app123")
            };

            var result = await controller.LinkPortfolioUser(88);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LinkPortfolioUserResponseDto>(ok.Value);

            Assert.Equal("NEW_TOKEN", response.Token);
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