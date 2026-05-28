using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using WebPagesDownloader;
using WebPagesDownloader.Extensions;
using WebPagesDownloader.Models;
using WebPagesDownloader.Services;


var services = new ServiceCollection();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();


services.Configure<DownloaderSettings>(
    configuration.GetSection("DownloaderSettings"));


services.AddHttpClient<WebPageDownloaderService>()
    .AddPolicyHandler(HttpClientPollyExtensions.GetRetryPolicy());


services.AddSingleton<IPageSaver, PageSaverService>();

var provider = services.BuildServiceProvider();
var downloader = provider.GetRequiredService<WebPageDownloaderService>();

var filePath = "urls.json";

if (!File.Exists(filePath))
{
    Console.WriteLine("urls.json not found!");
    return;
}

var json = await File.ReadAllTextAsync(filePath);

var config = JsonSerializer.Deserialize<UrlConfig>(json);

if (config?.Urls is null || config.Urls.Count == 0)
{
    Console.WriteLine("No URLs found.");
    return;
}

var startTime = DateTime.UtcNow;
Console.WriteLine("Starting downloads...\n");
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true; // prevents immediate termination
    cts.Cancel();    // triggers cancellation token
    Console.WriteLine("Cancellation requested...");
};

await foreach (var r in downloader.DownloadAllInChunksAsync(config.Urls, cts.Token))
{
    if (r.IsSuccess)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Ok {r.Url} - {r.ContentLength} bytes ({r.DurationMs:F0} ms)");
    }
        
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error :  {r.Url} - ERROR: {r.ErrorMessage}");
    }

    
    Console.ResetColor();
}
Console.WriteLine((DateTime.UtcNow - startTime).TotalMilliseconds);


