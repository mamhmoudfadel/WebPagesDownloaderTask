using Polly;
using System.Net;

namespace WebPagesDownloader.Extensions;
public static class HttpClientPollyExtensions
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r =>
                (int)r.StatusCode >= 500 ||
                r.StatusCode == HttpStatusCode.RequestTimeout)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(200 * attempt),
                onRetry: (outcome, delay, attempt, ctx) =>
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Retry {attempt} after {delay.TotalMilliseconds}ms");
                    Console.ResetColor();
                });
    }
}