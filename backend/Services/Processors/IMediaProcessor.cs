using StudentStudyAI.Models;

namespace StudentStudyAI.Services.Processors
{
    public interface IMediaProcessor
    {
        Task<string> ExtractAudioAsync(string filePath);
        Task<string> TranscribeAudioAsync(string filePath);
        Task<List<string>> ExtractFramesAsync(string filePath);
        Task<Dictionary<string, object>> ExtractMetadataAsync(string filePath);
    }
}
