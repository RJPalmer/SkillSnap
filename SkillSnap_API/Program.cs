using Microsoft.EntityFrameworkCore;
using SkillSnap_API.models;

using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

//add db context
builder.Services.AddDbContext<SkillSnapDbContext>(options =>
    options.UseSqlite("Data Source=skillsnap.db")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
