using System.Text;

namespace WebPagesDownloader.Extensions;

public static class UrlExtensions
{
    public static string ToSafeFileName(this string url)
    {
        var uri = new Uri(url);

        var builder = new StringBuilder();

        // host
        builder.Append(uri.Host);

        // path
        if (!string.IsNullOrWhiteSpace(uri.AbsolutePath) && uri.AbsolutePath != "/")
        {
            builder.Append(uri.AbsolutePath.Replace("/", "_"));
        }

        // query (optional but useful for uniqueness)
        if (!string.IsNullOrWhiteSpace(uri.Query))
        {
            builder.Append("_");
            builder.Append(uri.Query.TrimStart('?')
                                    .Replace("&", "_")
                                    .Replace("=", "-"));
        }

        var fileName = builder.ToString();

        // remove invalid file chars
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        // optional: limit length
        if (fileName.Length > 150)
            fileName = fileName[..150];

        return fileName + ".html";
    }
}