using System;

namespace NetworkAnalyzer.Models
{
    public class UrlHistory
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}