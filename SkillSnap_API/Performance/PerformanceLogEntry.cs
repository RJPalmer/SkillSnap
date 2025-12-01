namespace SkillSnap_API.Performance;

public class PerformanceLogEntry
{
    public string Path { get; set; } = "";
    public string Method { get; set; } = "";
    public long DurationMs { get; set; }
    public int StatusCode { get; set; }
    public string User { get; set; } = "";
    public DateTime Timestamp { get; set; }
}