using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudentStudyAI.Services;
using StudentStudyAI.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace StudentStudyAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, IDatabaseService databaseService, ILogger<UserController> logger)
        {
            _userService = userService;
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = "User account not found" });
                }

                var profile = new UserProfileResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive,
                    IsAdmin = user.IsAdmin
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new { error = "Profile retrieval failed", message = "An unexpected error occurred" });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Validation failed", details = ModelState });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = "User account not found" });
                }

                // Update user fields
                user.Username = request.Username ?? user.Username;
                user.Email = request.Email ?? user.Email;

                var success = await _userService.UpdateUserAsync(user);
                if (!success)
                {
                    return StatusCode(500, new { error = "Update failed", message = "Failed to update user profile" });
                }

                _logger.LogInformation("User profile updated: {UserId}", userId);
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { error = "Profile update failed", message = "An unexpected error occurred" });
            }
        }

        [HttpGet("study-sessions")]
        public async Task<IActionResult> GetStudySessions([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var sessions = await _databaseService.GetStudySessionsByUserIdAsync(userId.Value);
                
                // Apply pagination
                var paginatedSessions = sessions
                    .OrderByDescending(s => s.StartTime)
                    .Skip(offset)
                    .Take(limit)
                    .Select(s => new StudySessionResponse
                    {
                        Id = s.Id,
                        SessionName = s.SessionName,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Duration = s.Duration,
                        Notes = s.Notes,
                        FileIds = s.FileIds,
                        IsActive = s.IsActive
                    });

                var response = new
                {
                    sessions = paginatedSessions,
                    total = sessions.Count,
                    limit = limit,
                    offset = offset
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving study sessions for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, new { error = "Study sessions retrieval failed", message = "An unexpected error occurred" });
            }
        }

        [HttpPost("study-sessions")]
        public async Task<IActionResult> CreateStudySession([FromBody] CreateStudySessionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Validation failed", details = ModelState });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var session = new StudySession
                {
                    UserId = userId.Value,
                    SessionName = request.SessionName,
                    StartTime = DateTime.UtcNow,
                    Notes = request.Notes,
                    FileIds = request.FileIds ?? new List<int>(),
                    IsActive = true
                };

                var sessionId = await _databaseService.CreateStudySessionAsync(session);
                
                var response = new StudySessionResponse
                {
                    Id = sessionId,
                    SessionName = session.SessionName,
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    Duration = session.Duration,
                    Notes = session.Notes,
                    FileIds = session.FileIds,
                    IsActive = session.IsActive
                };

                _logger.LogInformation("Study session created: {SessionId} for user: {UserId}", sessionId, userId);
                return CreatedAtAction(nameof(GetStudySession), new { id = sessionId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating study session for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, new { error = "Study session creation failed", message = "An unexpected error occurred" });
            }
        }

        [HttpGet("study-sessions/{id}")]
        public async Task<IActionResult> GetStudySession(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var session = await _databaseService.GetStudySessionByIdAsync(id);
                if (session == null)
                {
                    return NotFound(new { error = "Study session not found", message = "Study session with specified ID does not exist" });
                }

                // Verify ownership
                if (session.UserId != userId.Value)
                {
                    return Forbid("You do not have permission to access this study session");
                }

                var response = new StudySessionResponse
                {
                    Id = session.Id,
                    SessionName = session.SessionName,
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    Duration = session.Duration,
                    Notes = session.Notes,
                    FileIds = session.FileIds,
                    IsActive = session.IsActive
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving study session: {SessionId}", id);
                return StatusCode(500, new { error = "Study session retrieval failed", message = "An unexpected error occurred" });
            }
        }

        [HttpPut("study-sessions/{id}")]
        public async Task<IActionResult> UpdateStudySession(int id, [FromBody] UpdateStudySessionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Validation failed", details = ModelState });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var session = await _databaseService.GetStudySessionByIdAsync(id);
                if (session == null)
                {
                    return NotFound(new { error = "Study session not found", message = "Study session with specified ID does not exist" });
                }

                // Verify ownership
                if (session.UserId != userId.Value)
                {
                    return Forbid("You do not have permission to update this study session");
                }

                // Update session fields
                session.SessionName = request.SessionName ?? session.SessionName;
                session.Notes = request.Notes ?? session.Notes;
                session.FileIds = request.FileIds ?? session.FileIds;
                session.IsActive = request.IsActive ?? session.IsActive;

                if (request.EndSession == true && session.EndTime == null)
                {
                    session.EndTime = DateTime.UtcNow;
                }

                var success = await _databaseService.UpdateStudySessionAsync(session);
                if (!success)
                {
                    return StatusCode(500, new { error = "Update failed", message = "Failed to update study session" });
                }

                _logger.LogInformation("Study session updated: {SessionId} for user: {UserId}", id, userId);
                return Ok(new { message = "Study session updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating study session: {SessionId}", id);
                return StatusCode(500, new { error = "Study session update failed", message = "An unexpected error occurred" });
            }
        }

        [HttpDelete("study-sessions/{id}")]
        public async Task<IActionResult> DeleteStudySession(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var session = await _databaseService.GetStudySessionByIdAsync(id);
                if (session == null)
                {
                    return NotFound(new { error = "Study session not found", message = "Study session with specified ID does not exist" });
                }

                // Verify ownership
                if (session.UserId != userId.Value)
                {
                    return Forbid("You do not have permission to delete this study session");
                }

                var success = await _databaseService.DeleteStudySessionAsync(id);
                if (!success)
                {
                    return StatusCode(500, new { error = "Delete failed", message = "Failed to delete study session" });
                }

                _logger.LogInformation("Study session deleted: {SessionId} for user: {UserId}", id, userId);
                return Ok(new { message = "Study session deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting study session: {SessionId}", id);
                return StatusCode(500, new { error = "Study session deletion failed", message = "An unexpected error occurred" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var files = await _databaseService.GetFileUploadsByUserIdAsync(userId.Value);
                var sessions = await _databaseService.GetStudySessionsByUserIdAsync(userId.Value);
                var studyGuides = await _databaseService.GetStudyGuidesByUserIdAsync(userId.Value);
                var quizzes = await _databaseService.GetQuizzesByUserIdAsync(userId.Value);
                var analysisResults = await _databaseService.GetAnalysisResultsByUserIdAsync(userId.Value);

                var totalStudyTime = sessions
                    .Where(s => s.EndTime.HasValue)
                    .Sum(s => s.Duration.TotalMinutes);

                var stats = new UserStatsResponse
                {
                    TotalFiles = files.Count,
                    TotalStudySessions = sessions.Count,
                    TotalStudyTimeMinutes = (int)totalStudyTime,
                    ActiveStudySessions = sessions.Count(s => s.IsActive),
                    StudyGuidesCreated = studyGuides.Count,
                    QuizzesTaken = quizzes.Count,
                    AnalysisResults = analysisResults.Count(),
                    FilesProcessed = files.Count(f => f.IsProcessed),
                    LastActivity = files.Any() ? files.Max(f => f.UploadedAt) : (DateTime?)null
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error retrieving user stats for user: {UserId}", userId);
                return StatusCode(500, new { 
                    error = "Stats retrieval failed", 
                    message = "An unexpected error occurred",
                    details = ex.Message,
                    userId = userId,
                    hasAuth = User.Identity?.IsAuthenticated ?? false
                });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            _logger.LogInformation("GetCurrentUserId - Claim found: {HasClaim}, Value: {ClaimValue}, IsAuthenticated: {IsAuth}", 
                userIdClaim != null, userIdClaim?.Value, User.Identity?.IsAuthenticated);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : null;
        }
    }

    // Request/Response Models
    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class UpdateProfileRequest
    {
        [MaxLength(50)]
        public string? Username { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }
    }

    public class StudySessionResponse
    {
        public int Id { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Notes { get; set; }
        public List<int> FileIds { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class CreateStudySessionRequest
    {
        [Required]
        [MaxLength(100)]
        public string SessionName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public List<int>? FileIds { get; set; }
    }

    public class UpdateStudySessionRequest
    {
        [MaxLength(100)]
        public string? SessionName { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public List<int>? FileIds { get; set; }
        public bool? IsActive { get; set; }
        public bool? EndSession { get; set; }
    }

    public class UserStatsResponse
    {
        public int TotalFiles { get; set; }
        public int TotalStudySessions { get; set; }
        public int TotalStudyTimeMinutes { get; set; }
        public int ActiveStudySessions { get; set; }
        public int StudyGuidesCreated { get; set; }
        public int QuizzesTaken { get; set; }
        public int AnalysisResults { get; set; }
        public int FilesProcessed { get; set; }
        public DateTime? LastActivity { get; set; }
    }
}
