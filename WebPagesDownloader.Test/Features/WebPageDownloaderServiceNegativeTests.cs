using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WebPagesDownloader.Services;

namespace WebPagesDownloader.Test.Features;

public class WebPageDownloaderServiceNegativeTests
{
    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenHttpReturnsNonSuccessStatus_ShouldReturnFailedResult(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[] { "https://example.com" };

        var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, "error");

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 1,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(httpClient, mockPageSaver.Object, settings);

        // Act
        var result = await sut.DownloadAllInChunksAsync(urls).FirstAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenResponseIsEmpty_ShouldStillReturnSuccess(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[] { "https://example.com" };

        var httpClient = CreateHttpClient(HttpStatusCode.OK, string.Empty);

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 1,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(httpClient, mockPageSaver.Object, settings);

        // Act
        var result = await sut.DownloadAllInChunksAsync(urls).FirstAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ContentLength.Should().Be(0);
    }

    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenMaxFileSizeExceeded_ShouldReturnTruncatedContentResult(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[] { "https://example.com" };

        var largeContent = new string('A', 5000);

        var httpClient = CreateHttpClient(HttpStatusCode.OK, largeContent);

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 1,
            MaxFileSizeInBytes = 100
        });

        var sut = new WebPageDownloaderService(httpClient, mockPageSaver.Object, settings);

        // Act
        var result = await sut.DownloadAllInChunksAsync(urls).FirstAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ContentLength.Should().BeLessOrEqualTo(100);
    }

    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenSavePageThrows_ShouldReturnFailedResult(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[] { "https://example.com" };

        mockPageSaver
            .Setup(x => x.SavePageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Disk full"));

        var httpClient = CreateHttpClient(HttpStatusCode.OK, "content");

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 1,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(httpClient, mockPageSaver.Object, settings);

        // Act
        var result = await sut.DownloadAllInChunksAsync(urls).FirstAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Disk full");
    }

    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenMultipleUrlsAndOneFails_ShouldStillReturnAllResults(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[]
        {
            "https://good.com",
            "https://bad.com"
        };

        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("ok")
            })
            .ThrowsAsync(new HttpRequestException("failure"));

        var httpClient = new HttpClient(handler.Object);

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 2,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(httpClient, mockPageSaver.Object, settings);

        // Act
        var results = await sut.DownloadAllInChunksAsync(urls).ToListAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Any(r => r.IsSuccess).Should().BeTrue();
        results.Any(r => !r.IsSuccess).Should().BeTrue();
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string content)
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });

        return new HttpClient(handler.Object);
    }
}