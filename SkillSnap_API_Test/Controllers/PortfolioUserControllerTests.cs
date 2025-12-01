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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SkillSnap_API_Test.Controllers
{
    public class PortfolioUserControllerTests
    {
        // ----------------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------------

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

            var options = Options.Create(new IdentityOptions());
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = new List<IUserValidator<ApplicationUser>>() { new Mock<IUserValidator<ApplicationUser>>().Object };
            var pwdValidators = new List<IPasswordValidator<ApplicationUser>>() { new Mock<IPasswordValidator<ApplicationUser>>().Object };
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new IdentityErrorDescriber();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                options,
                passwordHasher.Object,
                userValidators,
                pwdValidators,
                keyNormalizer.Object,
                errors,
                serviceProvider.Object,
                logger.Object
            );
        }

        private JwtTokenService CreateRealJwtService(UserManager<ApplicationUser> userManager)
        {
            var cfg = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            cfg.Setup(c => c["Jwt:Key"]).Returns(new string('x', 32));
            cfg.Setup(c => c["Jwt:Issuer"]).Returns("test");
            cfg.Setup(c => c["Jwt:Audience"]).Returns("test");
            cfg.Setup(c => c["Jwt:ExpiresInMinutes"]).Returns("60");

            return new JwtTokenService(cfg.Object, userManager);
        }

        private static HttpContext FakeHttpContextWithClaim(string type, string value)
        {
            var ctx = new DefaultHttpContext();

            ctx.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new List<Claim> { new Claim(type, value) },
                    authenticationType: "TestAuth"
                )
            );

            return ctx;
        }

        // ----------------------------------------------------------------------
        // TESTS: GET ALL
        // ----------------------------------------------------------------------
        [Fact]
        public async Task GetAll_ReturnsAllUsers()
        {
            var db = CreateDbContext();

            db.PortfolioUsers.Add(new PortfolioUser { Id = 1, Name = "U1", Bio = "B1", ProfileImageUrl = "" });
            db.PortfolioUsers.Add(new PortfolioUser { Id = 2, Name = "U2", Bio = "B2", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var result = await ctrl.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<PortfolioUserDto>>(ok.Value);

            Assert.Equal(2, dtos.Count());
        }

        // ----------------------------------------------------------------------
        // TESTS: GET BY ID
        // ----------------------------------------------------------------------
        [Fact]
        public async Task GetById_ReturnsUser_WhenExists()
        {
            var db = CreateDbContext();

            db.PortfolioUsers.Add(new PortfolioUser { Id = 10, Name = "TestUser", Bio = "Bio", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var result = await ctrl.GetById(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PortfolioUserDto>(ok.Value);
            Assert.Equal("TestUser", dto.Name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            var db = CreateDbContext();
            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var result = await ctrl.GetById(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ----------------------------------------------------------------------
        // TESTS: GET BY NAME
        // ----------------------------------------------------------------------
        [Fact]
        public async Task GetByName_ReturnsUser_WhenExists()
        {
            var db = CreateDbContext();

            db.PortfolioUsers.Add(new PortfolioUser { Id = 1, Name = "Alpha", Bio = "Bio", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var result = await ctrl.GetByName("Alpha");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PortfolioUserDto>(ok.Value);

            Assert.Equal("Alpha", dto.Name);
        }

        [Fact]
        public async Task GetByName_ReturnsNotFound_WhenMissing()
        {
            var db = CreateDbContext();
            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var result = await ctrl.GetByName("DoesNotExist");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ----------------------------------------------------------------------
        // TESTS: CREATE
        // ----------------------------------------------------------------------
        [Fact]
        public async Task Create_ReturnsCreated_WhenValid()
        {
            var db = CreateDbContext();
            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var input = new PortfolioUserCreateDto
            {
                Name = "NewUser",
                Bio = "Test Bio",
                ProfileImageUrl = "img.png"
            };

            var result = await ctrl.Create(input);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var dto = Assert.IsType<PortfolioUserDto>(created.Value);

            Assert.Equal("NewUser", dto.Name);
            Assert.Equal(1, db.PortfolioUsers.Count());
        }

        // ----------------------------------------------------------------------
        // TESTS: UPDATE
        // ----------------------------------------------------------------------
        [Fact]
        public async Task Update_ReturnsNoContent_WhenUpdated()
        {
            var db = CreateDbContext();

            db.PortfolioUsers.Add(new PortfolioUser { Id = 5, Name = "Old", Bio = "OldBio", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var input = new PortfolioUserCreateDto
            {
                Name = "Updated",
                Bio = "UpdatedBio",
                ProfileImageUrl = "img2.png"
            };

            var result = await ctrl.Update(5, input);

            Assert.IsType<NoContentResult>(result);

            var updated = await db.PortfolioUsers.FindAsync(5);
            Assert.Equal("Updated", updated.Name);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenMissing()
        {
            var db = CreateDbContext();
            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var input = new PortfolioUserCreateDto { Name = "DoesNotMatter" };

            var result = await ctrl.Update(999, input);

            Assert.IsType<NotFoundResult>(result);
        }

        // ----------------------------------------------------------------------
        // TESTS: DELETE
        // ----------------------------------------------------------------------
        [Fact]
        public async Task Delete_RemovesUser_WhenExists()
        {
            var db = CreateDbContext();
            db.PortfolioUsers.Add(new PortfolioUser { Id = 3, Name = "DeleteMe", Bio = "", ProfileImageUrl = "" });
            await db.SaveChangesAsync();

            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var result = await ctrl.Delete(3);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(db.PortfolioUsers);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            var db = CreateDbContext();
            var ctrl = new PortfolioUserController(db, CreateUserManagerMock().Object, CreateRealJwtService(CreateUserManagerMock().Object));

            var result = await ctrl.Delete(123);

            Assert.IsType<NotFoundResult>(result);
        }

        // ----------------------------------------------------------------------
        // TESTS: GET MY PROFILE
        // ----------------------------------------------------------------------
        [Fact]
        public async Task GetMyProfile_ReturnsProfile_WhenClaimMatches()
        {
            var db = CreateDbContext();

            var user = new PortfolioUser { Id = 55, Name = "Self", Bio = "UserBio", ProfileImageUrl = "" };
            db.PortfolioUsers.Add(user);
            await db.SaveChangesAsync();

            var userManager = CreateUserManagerMock();
            var jwt = CreateRealJwtService(userManager.Object);

            var ctrl = new PortfolioUserController(db, userManager.Object, jwt);

            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = FakeHttpContextWithClaim("portfolioUserId", "55")
            };

            var result = await ctrl.GetMyProfile();

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<PortfolioUserDto>(ok.Value);

            Assert.Equal("Self", dto.Name);
        }

        // ----------------------------------------------------------------------
        // TESTS: LINK PORTFOLIO USER
        // ----------------------------------------------------------------------
        [Fact]
        public async Task LinkPortfolioUser_LinksSuccessfully()
        {
            var db = CreateDbContext();

            db.PortfolioUsers.Add(new PortfolioUser { Id = 88, Name = "PU", Bio = "", ProfileImageUrl = "" });
            db.Users.Add(new ApplicationUser { Id = "app123", Email = "test@test.com" });
            await db.SaveChangesAsync();

            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("app123")).ReturnsAsync(db.Users.Single());
            userManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string>());

            var jwt = CreateRealJwtService(userManager.Object);

            var ctrl = new PortfolioUserController(db, userManager.Object, jwt);

            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = FakeHttpContextWithClaim(ClaimTypes.NameIdentifier, "app123")
            };

            var result = await ctrl.LinkPortfolioUser(88);

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<LinkPortfolioUserResponseDto>(ok.Value);

            Assert.Equal(88, dto.PortfolioUserId);
            Assert.False(string.IsNullOrWhiteSpace(dto.Token));
        }
    }
}