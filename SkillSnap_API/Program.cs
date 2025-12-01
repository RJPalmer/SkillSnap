using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap_API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SkillSnap_API.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------
// Database
// -------------------------------------------------------------
builder.Services.AddDbContext<SkillSnapDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")
    )
);

builder.Services.AddScoped<DataSeeder>();

// -------------------------------------------------------------
// In-Memory Caching
// -------------------------------------------------------------
builder.Services.AddMemoryCache(options =>
{
    // Set default cache size to 1000 entries
    options.CompactionPercentage = 0.25;
    options.SizeLimit = 1024 * 5; // 5 MB
});

// Configure cache options with expiration
builder.Services.Configure<MemoryCacheOptions>(options =>
{
    // Track statistics for monitoring (can be queried later)
    options.CompactionPercentage = 0.25;
});

// Register cache service
builder.Services.AddScoped<ICacheService, CacheService>();

// -------------------------------------------------------------
// Identity (ApplicationUser + IdentityRole)
// -------------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SkillSnapDbContext>()
.AddDefaultTokenProviders();

// -------------------------------------------------------------
// JWT Authentication
// -------------------------------------------------------------
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"] 
              ?? throw new Exception("JWT Key missing in configuration");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// -------------------------------------------------------------
// CORS (Blazor)
// -------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7053")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -------------------------------------------------------------
// Controllers + model state logging
// -------------------------------------------------------------
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        var builtInFactory = options.InvalidModelStateResponseFactory;

        options.InvalidModelStateResponseFactory = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var request = context.HttpContext.Request;

            logger.LogWarning("Invalid model state for request {Method} {Path}", request.Method, request.Path);
            logger.LogDebug("Headers: {Headers}", request.Headers);

            return builtInFactory(context);
        };
    });

// -------------------------------------------------------------
// Swagger / OpenAPI
// -------------------------------------------------------------
builder.Services.AddOpenApi();

// -------------------------------------------------------------
// Kestrel
// -------------------------------------------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7271, listenOptions => listenOptions.UseHttps());
    options.ListenLocalhost(5169); // HTTP fallback
});

// -------------------------------------------------------------
// Build App
// -------------------------------------------------------------
var app = builder.Build();

// -------------------------------------------------------------
// HTTP Pipeline
// -------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

  
app.UseCors("BlazorClient");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -------------------------------------------------------------
// Seed Data
// -------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();