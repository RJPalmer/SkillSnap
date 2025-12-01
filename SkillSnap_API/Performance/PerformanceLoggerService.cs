using System.Text;
using System.Text.Json;

namespace SkillSnap_API.Performance;

public class PerformanceLoggerService
{
    private readonly string _logPath;
    private readonly long _maxFileSizeBytes = 2 * 1024 * 1024; // 2 MB limit
    private readonly int _maxArchivedFiles = 5;

    public PerformanceLoggerService(IWebHostEnvironment env)
    {
        _logPath = Path.Combine(env.ContentRootPath, "Performance", "perf.log");

        var folder = Path.GetDirectoryName(_logPath);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
    }

    public async Task LogAsync(PerformanceLogEntry entry)
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

        var fileInfo = new FileInfo(_logPath);
        if (fileInfo.Length < _maxFileSizeBytes)
            return;

        // Rotate file
        string archiveName = $"perf_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log";
        string archivePath = Path.Combine(Path.GetDirectoryName(_logPath)!, archiveName);

        File.Move(_logPath, archivePath, overwrite: false);

        // Create a fresh file
        await File.WriteAllTextAsync(_logPath, "");

        CleanupOldArchives();
    }

    private void CleanupOldArchives()
    {
        string folder = Path.GetDirectoryName(_logPath)!;

        var archivedFiles = Directory.GetFiles(folder, "perf_*.log")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTimeUtc)
            .ToList();

        if (archivedFiles.Count <= _maxArchivedFiles)
            return;

        // delete oldest files
        foreach (var file in archivedFiles.Skip(_maxArchivedFiles))
        {
            try { file.Delete(); }
            catch { /* ignore */ }
        }
    }
}