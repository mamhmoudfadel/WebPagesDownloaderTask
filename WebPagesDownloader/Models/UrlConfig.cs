using System.Text.Json.Serialization;

namespace WebPagesDownloader.Models;


public class UrlConfig
{

    [JsonPropertyName("urls")] 
    public List<string> Urls { get; set; } = new();
}

