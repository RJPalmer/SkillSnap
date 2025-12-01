using System.Text;
using System.Text.Json;

namespace SkillSnap_Client.Performance;

public class PerformanceClientLoggerService
{
    private readonly string _logPath;
    private readonly long _maxFileSizeBytes = 1 * 1024 * 1024; // 1 MB
    private readonly int _maxArchivedFiles = 5;

    public PerformanceClientLoggerService()
    {
        var baseDir = AppContext.BaseDirectory;
        _logPath = Path.Combine(baseDir, "Performance", "perf-client.log");

        var folder = Path.GetDirectoryName(_logPath);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
    }

    public async Task LogAsync(ClientPerformanceEntry entry)
    {
        await RotateIfNeededAsync();

        var json = JsonSerializer.Serialize(entry);
        await File.AppendAllTextAsync(_logPath, json + Environment.NewLine, Encoding.UTF8);
    }

    // --------------------------
    // LOG ROTATION
    // --------------------------
    private async Task RotateIfNeededAsync()
    {
        if (!File.Exists(_logPath))
            return;

        var fi = new FileInfo(_logPath);
        if (fi.Length < _maxFileSizeBytes)
            return;

        string archiveName = $"perf-client_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log";
        string archivePath = Path.Combine(Path.GetDirectoryName(_logPath)!, archiveName);

        File.Move(_logPath, archivePath, overwrite: false);

        await File.WriteAllTextAsync(_logPath, "");

        CleanupOld();
    }

    private void CleanupOld()
    {
        string folder = Path.GetDirectoryName(_logPath)!;

        var archivedFiles = Directory.GetFiles(folder, "perf-client_*.log")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTimeUtc)
            .ToList();

        if (archivedFiles.Count <= _maxArchivedFiles)
            return;

        foreach (var file in archivedFiles.Skip(_maxArchivedFiles))
        {
            try { file.Delete(); }
            catch { }
        }
    }
}