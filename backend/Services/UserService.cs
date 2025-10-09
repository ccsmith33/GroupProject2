using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public class UserService
    {
        private readonly IDatabaseService _databaseService;
        private readonly JwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(IDatabaseService databaseService, JwtService jwtService, ILogger<UserService> logger)
        {
            _databaseService = databaseService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _databaseService.GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _databaseService.GetUserByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return null;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _databaseService.GetUserByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return null;
            }
        }

        public async Task<User> CreateUserAsync(RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with this email already exists");
                }

                existingUser = await GetUserByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with this username already exists");
                }

                // Create new user
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = _jwtService.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true,
                    IsAdmin = false
                };

                var userId = await _databaseService.CreateUserAsync(user);
                user.Id = userId;

                _logger.LogInformation("User created successfully: {Username} (ID: {UserId})", user.Username, userId);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", request.Username);
                throw;
            }
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("User not found or inactive: {Email}", email);
                    return false;
                }

                var isValid = _jwtService.VerifyPassword(password, user.PasswordHash);
                if (isValid)
                {
                    // Update last login time
                    await _databaseService.UpdateUserLastLoginAsync(user.Id);
                    _logger.LogInformation("User credentials validated: {Email}", email);
                }
                else
                {
                    _logger.LogWarning("Invalid password for user: {Email}", email);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user credentials: {Email}", email);
                return false;
            }
        }

        public async Task<User?> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return null;
                }

                var isValid = _jwtService.VerifyPassword(password, user.PasswordHash);
                if (isValid)
                {
                    // Update last login time
                    await _databaseService.UpdateUserLastLoginAsync(user.Id);
                    user.LastLoginAt = DateTime.UtcNow;
                    _logger.LogInformation("User authenticated successfully: {Email}", email);
                    return user;
                }

                _logger.LogWarning("Authentication failed for user: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user: {Email}", email);
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                await _databaseService.UpdateUserAsync(user);
                _logger.LogInformation("User updated successfully: {UserId}", user.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                await _databaseService.DeactivateUserAsync(userId);
                _logger.LogInformation("User deactivated: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return false;
            }
        }

        public UserInfo MapToUserInfo(User user)
        {
            return new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}