using Microsoft.AspNetCore.Http;

namespace StudentStudyAI.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, int userId);
        Task<Stream> GetFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        string GetContentType(string fileExtension);
    }
}
