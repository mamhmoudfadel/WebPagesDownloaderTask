using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WebPagesDownloader.Models;
using WebPagesDownloader.Services;

namespace WebPagesDownloader.Test.Features;

public class WebPageDownloaderServiceTests
{
    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenUrlReturnsSuccess_ShouldReturnSuccessfulResult(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[] { "https://example.com" };

        var httpClient = CreateHttpClient(HttpStatusCode.OK, "hello world");

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 2,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(
            httpClient,
            mockPageSaver.Object,
            settings);

        // Act
        var results = new List<DownloadResult>();

        await foreach (var result in sut.DownloadAllInChunksAsync(urls))
        {
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(1);
        results[0].IsSuccess.Should().BeTrue();
        results[0].Url.Should().Be(urls[0]);
    }

    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenRequestFails_ShouldReturnFailedResult(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[] { "https://badurl.com" };

        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handler.Object);

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 1,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(
            httpClient,
            mockPageSaver.Object,
            settings);

        // Act
        var results = new List<DownloadResult>();

        await foreach (var result in sut.DownloadAllInChunksAsync(urls))
        {
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(1);
        results[0].IsSuccess.Should().BeFalse();
        results[0].ErrorMessage.Should().Contain("Network error");
    }

    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenPageIsDownloaded_ShouldSavePage(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[] { "https://example.com" };

        var httpClient = CreateHttpClient(HttpStatusCode.OK, "saved content");

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 1,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(
            httpClient,
            mockPageSaver.Object,
            settings);

        // Act
        await foreach (var _ in sut.DownloadAllInChunksAsync(urls))
        {
        }

        // Assert
        mockPageSaver.Verify(
            x => x.SavePageAsync(
                urls[0],
                It.Is<string>(s => s.Contains("saved content"))),
            Times.Once);
    }

    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenMultipleUrlsProvided_ShouldReturnAllResults(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[]
        {
            "https://site1.com",
            "https://site2.com",
            "https://site3.com"
        };

        var httpClient = CreateHttpClient(HttpStatusCode.OK, "content");

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 2,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(
            httpClient,
            mockPageSaver.Object,
            settings);

        // Act
        var results = new List<DownloadResult>();

        await foreach (var result in sut.DownloadAllInChunksAsync(urls))
        {
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.IsSuccess);
    }

    [Theory, AutoMockData]
    public async Task DownloadAllInChunksAsync_WhenChunkSizeIsOne_ShouldProcessAllUrls(
        Mock<IPageSaver> mockPageSaver)
    {
        // Arrange
        var urls = new[]
        {
            "https://site1.com",
            "https://site2.com"
        };

        var httpClient = CreateHttpClient();

        var settings = Options.Create(new DownloaderSettings
        {
            ChunkSize = 1,
            MaxFileSizeInBytes = 1000
        });

        var sut = new WebPageDownloaderService(
            httpClient,
            mockPageSaver.Object,
            settings);

        // Act
        var results = new List<DownloadResult>();

        await foreach (var result in sut.DownloadAllInChunksAsync(urls))
        {
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(2);
    }

    private static HttpClient CreateHttpClient(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string content = "content")
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