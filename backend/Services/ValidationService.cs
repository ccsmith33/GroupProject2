using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace StudentStudyAI.Services
{
    public class ValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        public ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ValidationResult("Email is required");
            }

            if (!IsValidEmail(email))
            {
                return new ValidationResult("Invalid email format");
            }

            if (email.Length > 100)
            {
                return new ValidationResult("Email is too long (max 100 characters)");
            }

            return ValidationResult.Success;
        }

        public ValidationResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult("Password is required");
            }

            if (password.Length < 6)
            {
                return new ValidationResult("Password must be at least 6 characters long");
            }

            if (password.Length > 100)
            {
                return new ValidationResult("Password is too long (max 100 characters)");
            }

            // Check for common weak passwords
            var weakPasswords = new[] { "password", "123456", "qwerty", "abc123", "password123" };
            if (weakPasswords.Contains(password.ToLowerInvariant()))
            {
                return new ValidationResult("Password is too weak");
            }

            return ValidationResult.Success;
        }

        public ValidationResult ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new ValidationResult("Username is required");
            }

            if (username.Length < 3)
            {
                return new ValidationResult("Username must be at least 3 characters long");
            }

            if (username.Length > 50)
            {
                return new ValidationResult("Username is too long (max 50 characters)");
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$"))
            {
                return new ValidationResult("Username can only contain letters, numbers, underscores, and hyphens");
            }

            return ValidationResult.Success;
        }

        public ValidationResult ValidateFileUpload(IFormFile file)
        {
            if (file == null)
            {
                return new ValidationResult("File is required");
            }

            if (file.Length == 0)
            {
                return new ValidationResult("File is empty");
            }

            if (file.Length > 10 * 1024 * 1024) // 10MB
            {
                return new ValidationResult("File is too large (max 10MB)");
            }

            var allowedExtensions = new[] { ".pdf", ".txt", ".docx", ".doc", ".pptx", ".ppt", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".mp3", ".wav", ".mp4", ".avi" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return new ValidationResult("File type not supported");
            }

            // Check for malicious file names
            if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
            {
                return new ValidationResult("Invalid file name");
            }

            return ValidationResult.Success;
        }

        public ValidationResult ValidateStudyGuideRequest(string prompt, int userId)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return new ValidationResult("Prompt is required");
            }

            if (prompt.Length > 1000)
            {
                return new ValidationResult("Prompt is too long (max 1000 characters)");
            }

            if (userId <= 0)
            {
                return new ValidationResult("Invalid user ID");
            }

            // Check for potentially malicious content
            if (ContainsMaliciousContent(prompt))
            {
                return new ValidationResult("Prompt contains potentially harmful content");
            }

            return ValidationResult.Success;
        }

        public ValidationResult ValidateQuizRequest(string prompt, int userId)
        {
            return ValidateStudyGuideRequest(prompt, userId); // Same validation rules
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool ContainsMaliciousContent(string text)
        {
            var maliciousPatterns = new[]
            {
                @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", // Script tags
                @"javascript:", // JavaScript protocol
                @"on\w+\s*=", // Event handlers
                @"<iframe\b", // Iframe tags
                @"<object\b", // Object tags
                @"<embed\b" // Embed tags
            };

            foreach (var pattern in maliciousPatterns)
            {
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    _logger.LogWarning("Potentially malicious content detected: {Pattern}", pattern);
                    return true;
                }
            }

            return false;
        }
    }
}
