using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudentStudyAI.Services;
using StudentStudyAI.Models;
using System.Security.Claims;

namespace StudentStudyAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserService userService, JwtService jwtService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Validation failed", details = ModelState });
                }

                var user = await _userService.CreateUserAsync(request);
                var token = _jwtService.GenerateToken(user);
                var refreshToken = GenerateRefreshToken();

                // TODO: Store refresh token in database
                // await _databaseService.StoreRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

                var response = new AuthResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // TODO: Get from config
                    User = _userService.MapToUserInfo(user)
                };

                _logger.LogInformation("User registered successfully: {Username}", user.Username);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
                return BadRequest(new { error = "Registration failed", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { error = "Registration failed", message = "An unexpected error occurred" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Validation failed", details = ModelState });
                }

                var user = await _userService.AuthenticateUserAsync(request.Email, request.Password);
                if (user == null)
                {
                    _logger.LogWarning("Login failed for email: {Email}", request.Email);
                    return Unauthorized(new { error = "Invalid credentials", message = "Email or password is incorrect" });
                }

                var token = _jwtService.GenerateToken(user);
                var refreshToken = GenerateRefreshToken();

                // TODO: Store refresh token in database
                // await _databaseService.StoreRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

                var response = new AuthResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // TODO: Get from config
                    User = _userService.MapToUserInfo(user)
                };

                _logger.LogInformation("User logged in successfully: {Username}", user.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { error = "Login failed", message = "An unexpected error occurred" });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { error = "Invalid request", message = "Refresh token is required" });
                }

                // TODO: Validate refresh token from database
                // var refreshTokenData = await _databaseService.GetRefreshTokenAsync(request.RefreshToken);
                // if (refreshTokenData == null || refreshTokenData.IsRevoked || refreshTokenData.ExpiresAt < DateTime.UtcNow)
                // {
                //     return Unauthorized(new { error = "Invalid refresh token", message = "Refresh token is invalid or expired" });
                // }

                // TODO: Get user from refresh token
                // var user = await _userService.GetUserByIdAsync(refreshTokenData.UserId);
                // if (user == null || !user.IsActive)
                // {
                //     return Unauthorized(new { error = "User not found", message = "User account is not active" });
                // }

                // For now, return error as refresh token functionality needs database implementation
                return BadRequest(new { error = "Not implemented", message = "Refresh token functionality requires database implementation" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { error = "Token refresh failed", message = "An unexpected error occurred" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // TODO: Revoke refresh tokens for user
                    // await _databaseService.RevokeUserRefreshTokensAsync(userId);
                    _logger.LogInformation("User logged out: {UserId}", userId);
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { error = "Logout failed", message = "An unexpected error occurred" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = "User account not found" });
                }

                var userInfo = _userService.MapToUserInfo(user);
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, new { error = "User retrieval failed", message = "An unexpected error occurred" });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Validation failed", details = ModelState });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Unable to identify user" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = "User account not found" });
                }

                // Verify current password
                if (!_jwtService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new { error = "Invalid password", message = "Current password is incorrect" });
                }

                // Update password
                user.PasswordHash = _jwtService.HashPassword(request.NewPassword);
                await _userService.UpdateUserAsync(user);

                _logger.LogInformation("Password changed for user: {UserId}", userId);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { error = "Password change failed", message = "An unexpected error occurred" });
            }
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
