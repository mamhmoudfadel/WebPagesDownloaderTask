namespace WebPagesDownloader.Services
{
    public interface IPageSaver
    {
        Task SavePageAsync(string url, string content);
    }
}