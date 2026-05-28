using FluentAssertions;
using WebPagesDownloader.Services;

namespace WebPagesDownloader.Test;

public class PageSaverServiceTests
{
    [Fact]
    public async Task SavePageAsync_WhenValidInput_ShouldCreateFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var service = new PageSaverService(tempDir);

        var url = "https://example.com";
        var content = "<html>hello</html>";

        // Act
        await service.SavePageAsync(url, content);

        // Assert
        var expectedFile = Directory.GetFiles(tempDir).Single();
        File.Exists(expectedFile).Should().BeTrue();

        var savedContent = await File.ReadAllTextAsync(expectedFile);
        savedContent.Should().Be(content);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task SavePageAsync_WhenDirectoryDoesNotExist_ShouldCreateDirectory()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var service = new PageSaverService(tempDir);

        var url = "https://example.com";
        var content = "test";

        // Act
        await service.SavePageAsync(url, content);

        // Assert
        Directory.Exists(tempDir).Should().BeTrue();

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task SavePageAsync_WhenUrlContainsInvalidCharacters_ShouldStillCreateFileUsingSafeName()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var service = new PageSaverService(tempDir);

        var url = "https://example.com/?q=test&x=1";
        var content = "content";

        // Act
        await service.SavePageAsync(url, content);

        // Assert
        Directory.GetFiles(tempDir).Should().NotBeEmpty();

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task SavePageAsync_WhenCalledMultipleTimes_ShouldCreateMultipleFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var service = new PageSaverService(tempDir);

        // Act
        await service.SavePageAsync("https://site1.com", "a");
        await service.SavePageAsync("https://site2.com", "b");
        await service.SavePageAsync("https://site3.com", "c");

        // Assert
        Directory.GetFiles(tempDir).Should().HaveCount(3);

        // Cleanup
        Directory.Delete(tempDir, true);
    }  
}