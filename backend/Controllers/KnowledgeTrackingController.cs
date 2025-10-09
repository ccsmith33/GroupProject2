using Microsoft.AspNetCore.Mvc;
using StudentStudyAI.Models;
using StudentStudyAI.Services;

namespace StudentStudyAI.Controllers
{
    [ApiController]
    [Route("api/knowledge")]
    public class KnowledgeTrackingController : ControllerBase
    {
        private readonly KnowledgeTrackingService _knowledgeTrackingService;
        private readonly ILogger<KnowledgeTrackingController> _logger;

        public KnowledgeTrackingController(KnowledgeTrackingService knowledgeTrackingService, ILogger<KnowledgeTrackingController> logger)
        {
            _knowledgeTrackingService = knowledgeTrackingService;
            _logger = logger;
        }

        [HttpGet("{userId}/analytics")]
        public async Task<IActionResult> GetKnowledgeAnalytics(int userId)
        {
            try
            {
                _logger.LogInformation("Getting knowledge analytics for user {UserId}", userId);
                var analytics = await _knowledgeTrackingService.GetKnowledgeAnalyticsAsync(userId);
                return Ok(new { success = true, data = analytics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting knowledge analytics for user {UserId}", userId);
                return Problem($"Error getting knowledge analytics: {ex.Message}");
            }
        }

        [HttpPost("{userId}/level")]
        public async Task<IActionResult> UpdateKnowledgeLevel(int userId, [FromBody] KnowledgeTrackingRequest request)
        {
            try
            {
                _logger.LogInformation("Updating knowledge level for user {UserId} in subject {Subject}", userId, request.Subject);
                
                if (userId != request.UserId)
                {
                    return BadRequest("User ID mismatch");
                }

                if (string.IsNullOrWhiteSpace(request.Subject))
                {
                    return BadRequest("Subject is required");
                }

                if (!request.NewLevel.HasValue)
                {
                    return BadRequest("New level is required");
                }

                var profile = await _knowledgeTrackingService.UpdateKnowledgeLevelAsync(
                    userId, 
                    request.Subject, 
                    request.NewLevel.Value, 
                    request.ChangeReason ?? "manual_adjustment",
                    request.ConfidenceScore
                );

                return Ok(new { success = true, data = profile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating knowledge level for user {UserId}", userId);
                return Problem($"Error updating knowledge level: {ex.Message}");
            }
        }

        [HttpPost("performance")]
        public async Task<IActionResult> RecordQuizPerformance([FromBody] QuizPerformanceRequest request)
        {
            try
            {
                _logger.LogInformation("Recording quiz performance for user {UserId}, quiz {QuizId}", request.UserId, request.QuizId);
                
                var performance = await _knowledgeTrackingService.RecordQuizPerformanceAsync(
                    request.UserId, 
                    request.QuizId, 
                    request.Score,
                    request.TimeSpent
                );

                return Ok(new { success = true, data = performance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording quiz performance");
                return Problem($"Error recording quiz performance: {ex.Message}");
            }
        }

        [HttpGet("{userId}/preferences")]
        public async Task<IActionResult> GetLearningPreferences(int userId)
        {
            try
            {
                _logger.LogInformation("Getting learning preferences for user {UserId}", userId);
                var preferences = await _knowledgeTrackingService.GetUserLearningPreferencesAsync(userId);
                
                if (preferences == null)
                {
                    return NotFound("Learning preferences not found");
                }

                return Ok(new { success = true, data = preferences });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learning preferences for user {UserId}", userId);
                return Problem($"Error getting learning preferences: {ex.Message}");
            }
        }

        [HttpPut("{userId}/preferences")]
        public async Task<IActionResult> UpdateLearningPreferences(int userId, [FromBody] LearningPreferencesRequest request)
        {
            try
            {
                _logger.LogInformation("Updating learning preferences for user {UserId}", userId);
                
                var preferences = await _knowledgeTrackingService.UpdateLearningPreferencesAsync(userId, request);
                return Ok(new { success = true, data = preferences });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning preferences for user {UserId}", userId);
                return Problem($"Error updating learning preferences: {ex.Message}");
            }
        }

        [HttpPost("analyze-content/{fileId}")]
        public async Task<IActionResult> AnalyzeContentDifficulty(int fileId)
        {
            try
            {
                _logger.LogInformation("Analyzing content difficulty for file {FileId}", fileId);
                var analysis = await _knowledgeTrackingService.AnalyzeContentDifficultyAsync(fileId);
                return Ok(new { success = true, data = analysis });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing content difficulty for file {FileId}", fileId);
                return Problem($"Error analyzing content difficulty: {ex.Message}");
            }
        }

        [HttpGet("{userId}/progression")]
        public async Task<IActionResult> GetKnowledgeProgression(int userId)
        {
            try
            {
                _logger.LogInformation("Getting knowledge progression for user {UserId}", userId);
                var progression = await _knowledgeTrackingService.GetKnowledgeProgressionAsync(userId);
                return Ok(new { success = true, data = progression });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting knowledge progression for user {UserId}", userId);
                return Problem($"Error getting knowledge progression: {ex.Message}");
            }
        }
    }

    public class QuizPerformanceRequest
    {
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public decimal Score { get; set; }
        public int? TimeSpent { get; set; }
    }
}
