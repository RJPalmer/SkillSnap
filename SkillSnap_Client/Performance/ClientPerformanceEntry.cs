namespace SkillSnap_Client.Performance;

public class ClientPerformanceEntry
{
    public string Url { get; set; } = "";
    public string Method { get; set; } = "";
    public long DurationMs { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
}