using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string?> SaveFileAsync(IFormFile file, string relativeFolder);
        bool DeleteFile(string relativePath);
        bool FileExists(string relativePath);
    }

}
