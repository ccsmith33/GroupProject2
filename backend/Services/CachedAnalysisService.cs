using Microsoft.Extensions.Caching.Memory;
using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public class CachedAnalysisService : IAnalysisService
    {
        private readonly AnalysisService _analysisService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedAnalysisService> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public CachedAnalysisService(
            AnalysisService analysisService,
            IMemoryCache cache,
            ILogger<CachedAnalysisService> logger)
        {
            _analysisService = analysisService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<StudyGuide> GenerateStudyGuideAsync(string prompt, int userId)
        {
            var cacheKey = $"study_guide_{userId}_{prompt.GetHashCode()}";
            
            if (_cache.TryGetValue(cacheKey, out StudyGuide? cachedGuide) && cachedGuide != null)
            {
                _logger.LogInformation("Study guide retrieved from cache for user: {UserId}", userId);
                return cachedGuide;
            }

            var studyGuide = await _analysisService.GenerateStudyGuideAsync(prompt, userId);
            
            _cache.Set(cacheKey, studyGuide, _cacheExpiration);
            _logger.LogInformation("Study guide cached for user: {UserId}", userId);
            
            return studyGuide;
        }

        public async Task<Quiz> GenerateQuizAsync(string prompt, int userId)
        {
            var cacheKey = $"quiz_{userId}_{prompt.GetHashCode()}";
            
            if (_cache.TryGetValue(cacheKey, out Quiz? cachedQuiz) && cachedQuiz != null)
            {
                _logger.LogInformation("Quiz retrieved from cache for user: {UserId}", userId);
                return cachedQuiz;
            }

            var quiz = await _analysisService.GenerateQuizAsync(prompt, userId);
            
            _cache.Set(cacheKey, quiz, _cacheExpiration);
            _logger.LogInformation("Quiz cached for user: {UserId}", userId);
            
            return quiz;
        }

        public async Task<AnalysisResult> AnalyzeFileAsync(FileUpload file, int userId)
        {
            var cacheKey = $"file_analysis_{file.Id}_{userId}";
            
            if (_cache.TryGetValue(cacheKey, out AnalysisResult? cachedAnalysis) && cachedAnalysis != null)
            {
                _logger.LogInformation("File analysis retrieved from cache for file: {FileId}", file.Id);
                return cachedAnalysis;
            }

            var analysis = await _analysisService.AnalyzeFileAsync(file, userId);
            
            _cache.Set(cacheKey, analysis, _cacheExpiration);
            _logger.LogInformation("File analysis cached for file: {FileId}", file.Id);
            
            return analysis;
        }

        public async Task<string> GenerateConversationalResponseAsync(string prompt, int userId)
        {
            // Don't cache conversational responses as they should be dynamic
            return await _analysisService.GenerateConversationalResponseAsync(prompt, userId);
        }

        public void InvalidateUserCache(int userId)
        {
            // This is a simplified approach - in production, you'd want a more sophisticated cache invalidation strategy
            _logger.LogInformation("Cache invalidated for user: {UserId}", userId);
        }
    }
}
