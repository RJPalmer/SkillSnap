using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SkillSnap_API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SkillSnap_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SkillSnapDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SkillSnapDbContext") ?? throw new InvalidOperationException("Connection string 'SkillSnapDbContext' not found.")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<SkillSnapDbContext>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7271, listenOptions =>
    {
        listenOptions.UseHttps();
    });
    options.ListenLocalhost(5169);
}); 
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});

// Add CORS policy for Blazor WASM client
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7053") // Update with your Blazor client URL
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add controllers
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        var builtInFactory = options.InvalidModelStateResponseFactory;

        options.InvalidModelStateResponseFactory = context =>
        {
            // Custom behavior can be added here
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Invalid model state detected.");
            logger.LogInformation("Request details: {RequestDetails}", context.HttpContext.Request);
            logger.LogDebug("Request body: {RequestBody}", context.HttpContext.Request.Body);
            logger.LogTrace("Request headers: {RequestHeaders}", context.HttpContext.Request.Headers);
            logger.LogCritical("Critical error occurred.");
            logger.LogError("An error occurred while processing the request.");
            logger.LogDebug("Debugging information: {DebugInfo}", context.HttpContext.Request);

            // Fallback to the built-in behavior
            return builtInFactory(context);
        };
    });

//Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
   options.Password.RequiredLength = 6;
   options.User.RequireUniqueEmail = true; 
})
.AddEntityFrameworkStores<SkillSnapDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});
// Configure DbContext with connection string from appsettings.json
builder.Services.AddDbContext<SkillSnapDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("BlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
