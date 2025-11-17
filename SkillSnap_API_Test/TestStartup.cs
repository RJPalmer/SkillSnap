using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillSnap_API.Data;

namespace SkillSnap_API_Test
{
    public class TestStartup
    {
        public static ServiceProvider InitializeServices()
        {
            var services = new ServiceCollection();

            // Add DbContext
            services.AddDbContext<SkillSnapDbContext>(options =>
                options.UseInMemoryDatabase("SkillSnap_TestDB"));

            // Add any required services (mock or real)
            // e.g. services.AddScoped<IUserService, UserService>();

            return services.BuildServiceProvider();
        }
    }
}