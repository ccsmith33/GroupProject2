using Microsoft.AspNetCore.Mvc;
using StudentStudyAI.Services;

namespace StudentStudyAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string subject, string studentLevel)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file provided", message = "Please select a file to upload" });
                }

                // Validate file type
                var allowedTypes = new[] { ".pdf", ".txt", ".docx", ".pptx", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedTypes.Contains(fileExtension))
                {
                    return BadRequest(new { 
                        error = "Invalid file type", 
                        message = "Only PDF, text, Word, PowerPoint, and image files are supported" 
                    });
                }

                // Validate file size (10MB limit)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { 
                        error = "File too large", 
                        message = "File size must be less than 10MB" 
                    });
                }

                // Mock file processing
                var fileId = Guid.NewGuid().ToString();
                var fileName = file.FileName;
                var fileType = fileExtension;
                var fileSize = file.Length;

                // Simulate processing time
                await Task.Delay(1000);

                var response = new
                {
                    fileId = fileId,
                    fileName = fileName,
                    fileType = fileType,
                    fileSize = fileSize,
                    uploadedAt = DateTime.UtcNow,
                    status = "ready"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Upload failed", message = ex.Message });
            }
        }

        [HttpGet("download/{fileId}")]
        public IActionResult DownloadFile(string fileId)
        {
            // Mock file download
            return NotFound(new { error = "File not found", message = "File download not implemented yet" });
        }

        [HttpDelete("{fileId}")]
        public IActionResult DeleteFile(string fileId)
        {
            // Mock file deletion
            return Ok(new { message = "File deleted successfully" });
        }

        [HttpGet("list")]
        public IActionResult GetFileList()
        {
            // Mock file list
            var files = new[]
            {
                new { fileId = "mock-001", fileName = "calculus_homework.pdf", fileType = ".pdf", uploadedAt = DateTime.UtcNow.AddDays(-1) },
                new { fileId = "mock-002", fileName = "math_notes.docx", fileType = ".docx", uploadedAt = DateTime.UtcNow.AddDays(-2) },
                new { fileId = "mock-003", fileName = "practice_problems.txt", fileType = ".txt", uploadedAt = DateTime.UtcNow.AddDays(-3) }
            };

            return Ok(files);
        }
    }
}
