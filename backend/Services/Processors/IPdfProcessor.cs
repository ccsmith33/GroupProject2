using StudentStudyAI.Models;

namespace StudentStudyAI.Services.Processors
{
    public interface IPdfProcessor
    {
        Task<string> ExtractTextAsync(string filePath);
        Task<List<string>> ExtractImagesAsync(string filePath);
        Task<Dictionary<string, object>> ExtractMetadataAsync(string filePath);
    }
}
