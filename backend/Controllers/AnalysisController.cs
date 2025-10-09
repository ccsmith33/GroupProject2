using Microsoft.AspNetCore.Mvc;
using StudentStudyAI.Models;
using StudentStudyAI.Services;

namespace StudentStudyAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly IDatabaseService _databaseService;

        public AnalysisController(IAnalysisService analysisService, IDatabaseService databaseService)
        {
            _analysisService = analysisService;
            _databaseService = databaseService;
        }

        [HttpPost("analyze-file/{fileId}")]
        public async Task<IActionResult> AnalyzeFile(int fileId, [FromQuery] int userId = 1)
        {
            try
            {
                var file = await _databaseService.GetFileByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound(new { error = "File not found", message = "The specified file does not exist" });
                }

                var analysis = await _analysisService.AnalyzeFileAsync(file, userId);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Analysis failed", message = ex.Message });
            }
        }

        [HttpPost("generate-study-guide")]
        public async Task<IActionResult> GenerateStudyGuide([FromBody] StudyGuideRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserPrompt))
                {
                    return BadRequest(new { error = "Prompt is required", message = "Please provide a prompt for study guide generation" });
                }

                var studyGuide = await _analysisService.GenerateStudyGuideAsync(request.UserPrompt, 1); // Default userId for now
                return Ok(studyGuide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Study guide generation failed", message = ex.Message });
            }
        }

        [HttpPost("generate-quiz")]
        public async Task<IActionResult> GenerateQuiz([FromBody] QuizRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserPrompt))
                {
                    return BadRequest(new { error = "Prompt is required", message = "Please provide a prompt for quiz generation" });
                }

                var quiz = await _analysisService.GenerateQuizAsync(request.UserPrompt, 1); // Default userId for now
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Quiz generation failed", message = ex.Message });
            }
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest(new { error = "Message is required", message = "Please provide a message" });
                }

                var response = await _analysisService.GenerateConversationalResponseAsync(request.Message, 1); // Default userId for now
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Chat failed", message = ex.Message });
            }
        }

        [HttpGet("study-guides/{userId}")]
        public async Task<IActionResult> GetStudyGuides(int userId)
        {
            try
            {
                var studyGuides = await _databaseService.GetStudyGuidesByUserIdAsync(userId);
                return Ok(studyGuides);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve study guides", message = ex.Message });
            }
        }

        [HttpGet("quizzes/{userId}")]
        public async Task<IActionResult> GetQuizzes(int userId)
        {
            try
            {
                var quizzes = await _databaseService.GetQuizzesByUserIdAsync(userId);
                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve quizzes", message = ex.Message });
            }
        }

        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetConversations(int userId, [FromQuery] int limit = 10)
        {
            try
            {
                var conversations = await _databaseService.GetConversationsByUserIdAsync(userId, limit);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve conversations", message = ex.Message });
            }
        }

        [HttpPost("submit-quiz-attempt")]
        public async Task<IActionResult> SubmitQuizAttempt([FromBody] QuizAttemptRequest request)
        {
            try
            {
                var attempt = new QuizAttempt
                {
                    QuizId = request.QuizId,
                    UserId = 1, // Default userId for now
                    Answers = System.Text.Json.JsonSerializer.Serialize(request.Answers),
                    Score = 0, // Default score, will be calculated later
                    CompletedAt = DateTime.UtcNow
                };

                var attemptId = await _databaseService.CreateQuizAttemptAsync(attempt);
                return Ok(new { attemptId, score = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to submit quiz attempt", message = ex.Message });
            }
        }
    }

}
