using StudentStudyAI.Models;
using StudentStudyAI.Utils;
using System.Text.RegularExpressions;

namespace StudentStudyAI.Services
{
    public class FileValidator
    {
        private readonly ILogger<FileValidator> _logger;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;
        private readonly string[] _allowedMimeTypes;

        public FileValidator(ILogger<FileValidator> logger, IConfiguration configuration)
        {
            _logger = logger;
            _maxFileSize = configuration.GetValue<long>("FileUpload:MaxSizeBytes", 10 * 1024 * 1024); // 10MB default
            _allowedExtensions = configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() ?? 
                new[] { ".pdf", ".doc", ".docx", ".txt", ".md", ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mp3", ".wav" };
            _allowedMimeTypes = configuration.GetSection("FileUpload:AllowedMimeTypes").Get<string[]>() ?? 
                new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 
                       "text/plain", "text/markdown", "image/jpeg", "image/png", "image/gif", "video/mp4", "audio/mpeg", "audio/wav" };
        }

        /// <summary>
        /// Validate and sanitize a file upload
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="userId">The user ID uploading the file</param>
        /// <returns>Validation result with sanitized filename</returns>
        public FileValidationResult ValidateFile(IFormFile file, int userId)
        {
            try
            {
                // Check if file exists
                if (file == null || file.Length == 0)
                {
                    return new FileValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "No file provided"
                    };
                }

                // Check file size
                if (file.Length > _maxFileSize)
                {
                    return new FileValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB"
                    };
                }

                // Sanitize filename
                var sanitizedFileName = SanitizeFileName(file.FileName);
                if (string.IsNullOrEmpty(sanitizedFileName))
                {
                    return new FileValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Invalid filename"
                    };
                }

                // Validate file extension
                var extension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    return new FileValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}"
                    };
                }

                // Validate MIME type
                if (!string.IsNullOrEmpty(file.ContentType) && !_allowedMimeTypes.Contains(file.ContentType))
                {
                    _logger.LogWarning("File {FileName} has suspicious MIME type {MimeType}", sanitizedFileName, file.ContentType);
                    // Don't reject based on MIME type alone as it can be spoofed, but log it
                }

                // Additional security checks
                if (ContainsPathTraversal(sanitizedFileName))
                {
                    return new FileValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Filename contains invalid characters"
                    };
                }

                // Generate unique filename to prevent conflicts
                var uniqueFileName = GenerateUniqueFileName(sanitizedFileName, userId);

                return new FileValidationResult
                {
                    IsValid = true,
                    SanitizedFileName = uniqueFileName,
                    OriginalFileName = file.FileName,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file {FileName}", file?.FileName);
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File validation failed"
                };
            }
        }

        /// <summary>
        /// Sanitize filename to prevent security issues
        /// </summary>
        /// <param name="fileName">Original filename</param>
        /// <returns>Sanitized filename</returns>
        public string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // Remove path traversal attempts
            fileName = fileName.Replace("..", "");
            fileName = fileName.Replace("\\", "");
            fileName = fileName.Replace("/", "");

            // Remove control characters and other dangerous characters
            fileName = Regex.Replace(fileName, @"[\x00-\x1F\x7F-\x9F]", "");

            // Remove or replace dangerous characters
            fileName = Regex.Replace(fileName, @"[<>:""|?*]", "_");

            // Limit length
            var extension = Path.GetExtension(fileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            
            if (nameWithoutExt.Length > 200)
            {
                nameWithoutExt = nameWithoutExt.Substring(0, 200);
            }

            fileName = nameWithoutExt + extension;

            // Ensure filename is not empty after sanitization
            if (string.IsNullOrWhiteSpace(fileName) || fileName == extension)
            {
                fileName = "file" + extension;
            }

            return fileName;
        }

        /// <summary>
        /// Check if filename contains path traversal attempts
        /// </summary>
        /// <param name="fileName">Filename to check</param>
        /// <returns>True if contains path traversal</returns>
        private bool ContainsPathTraversal(string fileName)
        {
            return fileName.Contains("..") || 
                   fileName.Contains("\\") || 
                   fileName.Contains("/") ||
                   fileName.StartsWith(".") ||
                   fileName.Contains(":") ||
                   fileName.Contains("|");
        }

        /// <summary>
        /// Generate unique filename to prevent conflicts
        /// </summary>
        /// <param name="fileName">Original filename</param>
        /// <param name="userId">User ID</param>
        /// <returns>Unique filename</returns>
        private string GenerateUniqueFileName(string fileName, int userId)
        {
            var extension = Path.GetExtension(fileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = Guid.NewGuid().ToString("N")[..8];
            
            return $"{userId}_{timestamp}_{random}_{nameWithoutExt}{extension}";
        }

        /// <summary>
        /// Validate file content for security
        /// </summary>
        /// <param name="file">File to validate</param>
        /// <returns>True if content is safe</returns>
        public async Task<bool> ValidateFileContent(IFormFile file)
        {
            try
            {
                // Read first few bytes to check for magic numbers
                using var stream = file.OpenReadStream();
                var buffer = new byte[Math.Min(1024, (int)file.Length)];
                await stream.ReadAsync(buffer, 0, buffer.Length);

                // Check for executable file signatures
                if (IsExecutableFile(buffer))
                {
                    _logger.LogWarning("File {FileName} appears to be an executable file", file.FileName);
                    return false;
                }

                // Check for script files
                if (IsScriptFile(buffer, file.FileName))
                {
                    _logger.LogWarning("File {FileName} appears to be a script file", file.FileName);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file content for {FileName}", file.FileName);
                return false;
            }
        }

        /// <summary>
        /// Check if file is an executable
        /// </summary>
        private bool IsExecutableFile(byte[] buffer)
        {
            // Check for common executable signatures
            if (buffer.Length >= 2)
            {
                // PE executable (Windows)
                if (buffer[0] == 0x4D && buffer[1] == 0x5A) return true;
                
                // ELF executable (Linux)
                if (buffer[0] == 0x7F && buffer[1] == 0x45 && buffer[2] == 0x4C && buffer[3] == 0x46) return true;
            }

            return false;
        }

        /// <summary>
        /// Check if file is a script
        /// </summary>
        private bool IsScriptFile(byte[] buffer, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var scriptExtensions = new[] { ".exe", ".bat", ".cmd", ".com", ".scr", ".pif", ".vbs", ".js", ".jse", ".wsf", ".wsh" };
            
            if (scriptExtensions.Contains(extension))
            {
                return true;
            }

            // Check for script signatures in content
            var content = System.Text.Encoding.UTF8.GetString(buffer);
            var scriptKeywords = new[] { "<script", "javascript:", "vbscript:", "onload=", "onerror=", "eval(", "function(" };
            
            return scriptKeywords.Any(keyword => content.ToLowerInvariant().Contains(keyword));
        }
    }

    /// <summary>
    /// Result of file validation
    /// </summary>
    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string SanitizedFileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}
