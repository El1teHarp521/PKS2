using System;

namespace MonitorSys.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Type { get; set; } = string.Empty;

        public override string ToString() => 
            $"[{Timestamp:HH:mm:ss}] {Type} | {Method} | {Url} | Status: {StatusCode}";
    }
}