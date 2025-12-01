using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkillSnap_Client.Performance;

public class PerformanceHttpHandler : DelegatingHandler
{
    private readonly PerformanceClientLoggerService _logger;

    public PerformanceHttpHandler(PerformanceClientLoggerService logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var response = await base.SendAsync(request, cancellationToken);

        sw.Stop();

        await _logger.LogAsync(new ClientPerformanceEntry
        {
            Url = request.RequestUri?.ToString() ?? "",
            Method = request.Method.Method,
            DurationMs = sw.ElapsedMilliseconds,
            StatusCode = (int)response.StatusCode,
            Timestamp = DateTime.UtcNow
        });

        return response;
    }
}