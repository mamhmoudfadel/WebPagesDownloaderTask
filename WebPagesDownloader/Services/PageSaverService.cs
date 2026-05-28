using System.Text;
using WebPagesDownloader.Extensions;

namespace WebPagesDownloader.Services
{
    public class PageSaverService : IPageSaver
    {
        private readonly string _saveDirectory;

        public PageSaverService(string saveDirectory = "saved_pages")
        {
            _saveDirectory = saveDirectory;
            // Ensure the directory exists
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        public async Task SavePageAsync(string url, string content)
        {
            try
            {                
                string fileName = url.ToSafeFileName() + ".html";
                string filePath = Path.Combine(_saveDirectory, fileName);

                
                await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
            }
            catch (Exception ex)
            {               
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error saving page for URL '{url}': {ex.Message}");
                Console.ResetColor();
                throw; 
            }
        }
    }
}