using KidNest.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace KidNest.Services.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> SaveFileAsync(IFormFile file, string relativePath)
        {
            if (file == null || file.Length == 0)
                return null;

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
                return null;

            string uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Defensive fix: normalize the relative path
            relativePath = relativePath.TrimStart('/', '\\');

            string fullFolderPath = Path.Combine(_env.WebRootPath, relativePath);
            Directory.CreateDirectory(fullFolderPath); // creates all intermediate folders if needed

            string fullPath = Path.Combine(fullFolderPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("/", relativePath.Replace('\\', '/'), uniqueFileName).Replace("\\", "/");
        }

        public bool DeleteFile(string relativePath)
        {
            string fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);

                return true;
            }

            return false;
        }

        public bool FileExists(string relativePath)
        {
            string fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            return File.Exists(fullPath);
        }
    }
}
