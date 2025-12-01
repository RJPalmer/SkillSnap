using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SkillSnap_API.Performance;

public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PerformanceLoggerService _logger;

    public PerformanceMiddleware(RequestDelegate next, PerformanceLoggerService logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        var userId = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity?.Name
            : "Anonymous";

        await _logger.LogAsync(new PerformanceLogEntry
        {
            Path = context.Request.Path,
            Method = context.Request.Method,
            DurationMs = sw.ElapsedMilliseconds,
            StatusCode = context.Response.StatusCode,
            User = userId,
            Timestamp = DateTime.UtcNow
        });
    }
}