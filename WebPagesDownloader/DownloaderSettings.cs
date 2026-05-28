namespace WebPagesDownloader
{
    public class DownloaderSettings
    {
        public int ChunkSize { get; set; } = 10;
        public int MaxFileSizeInBytes { get; set; } = 100 * 1024; // 100 KB
    }
}