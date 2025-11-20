using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using SkillSnap_API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SkillSnap_API.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------------
// Configure DbContext
// --------------------------
builder.Services.AddDbContext<SkillSnapDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// --------------------------
// Configure Identity
// --------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SkillSnapDbContext>()
.AddDefaultTokenProviders();

// --------------------------
// Configure JWT Authentication
// --------------------------
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

// --------------------------
// Configure CORS for Blazor client
// --------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7053") // Update to your Blazor client URL
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --------------------------
// Add Controllers & Configure ModelState logging
// --------------------------
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        var builtInFactory = options.InvalidModelStateResponseFactory;
        options.InvalidModelStateResponseFactory = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var request = context.HttpContext.Request;

            logger.LogWarning("Invalid model state detected for request {Method} {Path}", request.Method, request.Path);
            logger.LogDebug("Request headers: {Headers}", request.Headers);

            return builtInFactory(context);
        };
    });

// --------------------------
// Add OpenAPI / Swagger
// --------------------------
builder.Services.AddOpenApi();

// --------------------------
// Configure Kestrel
// --------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7271, listenOptions =>
    {
        listenOptions.UseHttps();
    });
    options.ListenLocalhost(5169);
});

var app = builder.Build();

// --------------------------
// Configure HTTP pipeline
// --------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("BlazorClient");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();