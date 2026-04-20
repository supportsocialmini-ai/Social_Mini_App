using Microsoft.AspNetCore.Http;

namespace Social_Mini_App.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string subDirectory);
        void DeleteFile(string filePath);
    }
}
