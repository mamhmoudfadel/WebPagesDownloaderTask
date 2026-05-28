using System.Text;
namespace WebPagesDownloader.Extensions;

public static class StreamReaderExtensions
{
    public static async Task<(string Content, int ByteLength)> ReadMaxContentAsync(
        this StreamReader reader,
        int maxChars = 100_000)
    {
        var buffer = new char[maxChars];

        int charsRead = await reader.ReadAsync(buffer);

        var content = new string(buffer, 0, charsRead);

        int byteLength = Encoding.UTF8.GetByteCount(content);

        return (content, byteLength);
    }
}

