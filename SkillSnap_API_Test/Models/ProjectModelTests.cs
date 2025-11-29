using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillSnap_API.Data;
using SkillSnap.Shared.Models;
using Xunit;

namespace SkillSnap_API_Test.Models
{
    public class ProjectModelTests
    {
        private SkillSnapDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SkillSnapDbContext>()
                .UseInMemoryDatabase(databaseName: $"SkillSnap_ProjectModelTest_{System.Guid.NewGuid()}")
                .Options;

            var context = new SkillSnapDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void CanInstantiateProject()
        {
            // Arrange & Act
            var project = new Project
            {
                Title = "Tetris Game App",
                Description = "A retro tetris game built with C# and Blazor",
                ImageUrl = "http://example.com/tetris.png",
            };

            // Assert
            Assert.Equal("Tetris Game App", project.Title);
            Assert.Equal("A retro tetris game built with C# and Blazor", project.Description);
            Assert.Equal("http://example.com/tetris.png", project.ImageUrl);
            //Assert.Equal(1, project.PortfolioUser.Id);
        }

        [Fact]
        public async Task CanAddProjectToDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var project = new Project
            {
                Title = "Portfolio Site",
                Description = "A personal site showcasing my work",
                ImageUrl = "http://example.com/portfolio.png",

            };

            // Act
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, await context.Projects.CountAsync());
            Assert.Equal("Portfolio Site", (await context.Projects.FirstAsync()).Title);
        }

        [Fact]
        public async Task DefaultPortfolioUserCollectionIsEmpty()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var project = new Project
            {
                Title = "War Card Game",
                Description = "A simple card game implementation",
                ImageUrl = "http://example.com/war.png"
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            // Act
            var result = await context.Projects
                .Include(p => p.PortfolioUserProjects)
                .FirstOrDefaultAsync();

            // Assert
            Assert.Empty(result!.PortfolioUserProjects ?? new System.Collections.Generic.List<PortfolioUserProject>());
        }
    }
}