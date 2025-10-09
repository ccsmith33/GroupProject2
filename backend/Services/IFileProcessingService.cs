using StudentStudyAI.Models;
using Microsoft.AspNetCore.Http;

namespace StudentStudyAI.Services
{
    public interface IFileProcessingService
    {
        Task<ProcessedFile> ProcessFileAsync(FileUpload file);
        Task<bool> ValidateFileAsync(IFormFile file);
    }
}
