namespace WebPagesDownloader.Models
{
    public class DownloadResult
    {
        public string Url { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public long ContentLength { get; set; }
        public double DurationMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
