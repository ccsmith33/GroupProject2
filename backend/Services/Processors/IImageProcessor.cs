using StudentStudyAI.Models;

namespace StudentStudyAI.Services.Processors
{
    public interface IImageProcessor
    {
        Task<string> ExtractTextAsync(string filePath);
        Task<List<string>> ExtractTextRegionsAsync(string filePath);
        Task<Dictionary<string, object>> AnalyzeImageAsync(string filePath);
    }
}
