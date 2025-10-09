using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public interface IAnalysisService
    {
        Task<StudyGuide> GenerateStudyGuideAsync(string prompt, int userId);
        Task<Quiz> GenerateQuizAsync(string prompt, int userId);
        Task<AnalysisResult> AnalyzeFileAsync(FileUpload file, int userId);
        Task<string> GenerateConversationalResponseAsync(string prompt, int userId);
    }
}
