using Microsoft.Extensions.Options;
using WebPagesDownloader.Extensions;
using WebPagesDownloader.Models;
namespace WebPagesDownloader.Services;

public class WebPageDownloaderService
{
    private readonly HttpClient _httpClient;
    private readonly IPageSaver _pageSaver;
    private readonly DownloaderSettings _settings;

    public WebPageDownloaderService(
        HttpClient httpClient,
        IPageSaver pageSaver,
        IOptions<DownloaderSettings> options)
    {
        _httpClient = httpClient;
        _pageSaver = pageSaver;
        _settings = options.Value;
    }

    public async IAsyncEnumerable<DownloadResult> DownloadAllInChunksAsync(IEnumerable<string> urls,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        foreach (var chunk in urls.Chunk(_settings.ChunkSize))
        {
            var tasks = chunk.Select(url => DownloadSingleAsync(url, cancellationToken));

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                yield return result;
            }
        }
    }


    private async Task<DownloadResult> DownloadSingleAsync(string url,
         CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            using var response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            var (content, contentLength) = await reader.ReadMaxContentAsync(_settings.MaxFileSizeInBytes);

            // Save the page locally
            await _pageSaver.SavePageAsync(url, content);

            return new DownloadResult
            {
                Url = url,
                IsSuccess = true,
                ContentLength = contentLength,
                DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            return new DownloadResult
            {
                Url = url,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }
}