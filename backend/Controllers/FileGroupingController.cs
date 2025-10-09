using Microsoft.AspNetCore.Mvc;
using StudentStudyAI.Models;
using StudentStudyAI.Services;

namespace StudentStudyAI.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileGroupingController : ControllerBase
    {
        private readonly SubjectGroupService _subjectGroupService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IDatabaseService _databaseService;
        private readonly BackgroundJobService _backgroundJobService;
        private readonly ILogger<FileGroupingController> _logger;

        public FileGroupingController(
            SubjectGroupService subjectGroupService,
            IFileStorageService fileStorageService,
            IFileProcessingService fileProcessingService,
            IDatabaseService databaseService,
            BackgroundJobService backgroundJobService,
            ILogger<FileGroupingController> logger)
        {
            _subjectGroupService = subjectGroupService;
            _fileStorageService = fileStorageService;
            _fileProcessingService = fileProcessingService;
            _databaseService = databaseService;
            _backgroundJobService = backgroundJobService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string userId, [FromForm] string subject = "", [FromForm] string studentLevel = "")
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file provided", message = "Please select a file to upload" });
                }

                // Validate and convert userId
                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a valid number" });
                }

                // Validate file using processing service
                if (!await _fileProcessingService.ValidateFileAsync(file))
                {
                    return BadRequest(new { 
                        error = "Invalid file", 
                        message = "File validation failed. Please check file type and size." 
                    });
                }

                // Save file to storage
                var filePath = await _fileStorageService.SaveFileAsync(file, userIdInt);
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Create file upload record
                var fileUpload = new FileUpload
                {
                    UserId = userIdInt,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileType = fileExtension,
                    FileSize = file.Length,
                    Subject = subject,
                    StudentLevel = studentLevel,
                    UploadedAt = DateTime.UtcNow,
                    Status = "uploaded"
                };

                // Save to database
                var fileId = await _databaseService.CreateFileUploadAsync(fileUpload);

                // Queue file processing as background job
                var processingJob = new BackgroundJob
                {
                    JobType = JobType.FileProcessing,
                    Data = new Dictionary<string, object>
                    {
                        ["fileId"] = fileId,
                        ["userId"] = userIdInt
                    }
                };
                _backgroundJobService.EnqueueJob(processingJob);

                var response = new
                {
                    fileId = fileId,
                    fileName = file.FileName,
                    fileType = fileExtension,
                    fileSize = file.Length,
                    uploadedAt = DateTime.UtcNow,
                    status = "uploaded",
                    message = "File uploaded successfully. Processing will begin shortly."
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
                return StatusCode(500, new { error = "Upload failed", message = ex.Message });
            }
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile(int fileId)
        {
            try
            {
                var fileUpload = await _databaseService.GetFileUploadAsync(fileId);
                if (fileUpload == null)
                {
                    return NotFound(new { error = "File not found" });
                }

                return Ok(new
                {
                    fileId = fileUpload.Id,
                    fileName = fileUpload.FileName,
                    fileType = fileUpload.FileType,
                    fileSize = fileUpload.FileSize,
                    uploadedAt = fileUpload.UploadedAt,
                    status = fileUpload.Status,
                    processedAt = fileUpload.ProcessedAt,
                    hasContent = !string.IsNullOrEmpty(fileUpload.ExtractedContent)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file with ID: {FileId}", fileId);
                return StatusCode(500, new { error = "File retrieval failed", message = ex.Message });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetFiles([FromQuery] int userId)
        {
            try
            {
                var files = await _databaseService.GetFileUploadsByUserIdAsync(userId);
                var fileList = files.Select(f => new
                {
                    fileId = f.Id,
                    fileName = f.FileName,
                    fileType = f.FileType,
                    fileSize = f.FileSize,
                    uploadedAt = f.UploadedAt,
                    status = f.Status,
                    processedAt = f.ProcessedAt,
                    hasContent = !string.IsNullOrEmpty(f.ExtractedContent)
                }).ToList();

                return Ok(new { files = fileList, count = fileList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving files for user: {UserId}", userId);
                return StatusCode(500, new { error = "File list retrieval failed", message = ex.Message });
            }
        }

        [HttpGet("{fileId}/status")]
        public async Task<IActionResult> GetFileStatus(int fileId)
        {
            try
            {
                var fileUpload = await _databaseService.GetFileUploadAsync(fileId);
                if (fileUpload == null)
                {
                    return NotFound(new { error = "File not found" });
                }

                return Ok(new
                {
                    fileId = fileUpload.Id,
                    fileName = fileUpload.FileName,
                    status = fileUpload.Status,
                    processedAt = fileUpload.ProcessedAt,
                    hasContent = !string.IsNullOrEmpty(fileUpload.ExtractedContent)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file status for ID: {FileId}", fileId);
                return StatusCode(500, new { error = "Status retrieval failed", message = ex.Message });
            }
        }

        [HttpGet("{userId}/grouped")]
        public async Task<IActionResult> GetGroupedFiles(int userId)
        {
            try
            {
                _logger.LogInformation("Getting grouped files for user {UserId}", userId);
                var groupedFiles = await _subjectGroupService.GetGroupedFilesAsync(userId);
                return Ok(new { success = true, data = groupedFiles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grouped files for user {UserId}", userId);
                return Problem($"Error getting grouped files: {ex.Message}");
            }
        }

        [HttpPut("{fileId}/subject")]
        public async Task<IActionResult> UpdateFileSubject(int fileId, [FromBody] FileGroupingRequest request)
        {
            try
            {
                _logger.LogInformation("Updating file subject for file {FileId}", fileId);
                
                if (fileId != request.FileId)
                {
                    return BadRequest("File ID mismatch");
                }

                var success = await _subjectGroupService.UpdateFileGroupingAsync(fileId, request);
                if (success)
                {
                    return Ok(new { success = true, message = "File grouping updated successfully" });
                }
                else
                {
                    return NotFound("File not found or update failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating file subject for file {FileId}", fileId);
                return Problem($"Error updating file subject: {ex.Message}");
            }
        }

        [HttpPost("bulk-update")]
        public async Task<IActionResult> BulkUpdateFileGrouping([FromBody] BulkFileGroupingRequest request)
        {
            try
            {
                _logger.LogInformation("Bulk updating file grouping for {FileCount} files", request.FileIds.Count);
                
                if (!request.FileIds.Any())
                {
                    return BadRequest("No files specified for bulk update");
                }

                var success = await _subjectGroupService.BulkUpdateFileGroupingAsync(1, request); // TODO: Get from auth
                if (success)
                {
                    return Ok(new { success = true, message = "Bulk file grouping updated successfully" });
                }
                else
                {
                    return Problem("Some files could not be updated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating file grouping");
                return Problem($"Error bulk updating file grouping: {ex.Message}");
            }
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateSubjectGroup([FromBody] SubjectGroupRequest request)
        {
            try
            {
                _logger.LogInformation("Creating subject group '{GroupName}'", request.GroupName);
                
                if (string.IsNullOrWhiteSpace(request.GroupName))
                {
                    return BadRequest("Group name is required");
                }

                var subjectGroup = await _subjectGroupService.CreateSubjectGroupAsync(1, request); // TODO: Get from auth
                return CreatedAtAction(nameof(GetSubjectGroup), new { groupId = subjectGroup.Id }, 
                    new { success = true, data = subjectGroup });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject group");
                return Problem($"Error creating subject group: {ex.Message}");
            }
        }

        [HttpGet("groups/{groupId}")]
        public async Task<IActionResult> GetSubjectGroup(int groupId)
        {
            try
            {
                _logger.LogInformation("Getting subject group {GroupId}", groupId);
                var subjectGroup = await _subjectGroupService.GetSubjectGroupAsync(groupId);
                
                if (subjectGroup == null)
                {
                    return NotFound("Subject group not found");
                }

                return Ok(new { success = true, data = subjectGroup });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subject group {GroupId}", groupId);
                return Problem($"Error getting subject group: {ex.Message}");
            }
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetSubjectGroups(int userId)
        {
            try
            {
                _logger.LogInformation("Getting subject groups for user {UserId}", userId);
                var subjectGroups = await _subjectGroupService.GetSubjectGroupsByUserIdAsync(userId);
                return Ok(new { success = true, data = subjectGroups });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subject groups for user {UserId}", userId);
                return Problem($"Error getting subject groups: {ex.Message}");
            }
        }

        [HttpPut("groups/{groupId}")]
        public async Task<IActionResult> UpdateSubjectGroup(int groupId, [FromBody] SubjectGroupRequest request)
        {
            try
            {
                _logger.LogInformation("Updating subject group {GroupId}", groupId);
                
                if (string.IsNullOrWhiteSpace(request.GroupName))
                {
                    return BadRequest("Group name is required");
                }

                var subjectGroup = await _subjectGroupService.UpdateSubjectGroupAsync(groupId, request);
                if (subjectGroup == null)
                {
                    return NotFound("Subject group not found");
                }

                return Ok(new { success = true, data = subjectGroup });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject group {GroupId}", groupId);
                return Problem($"Error updating subject group: {ex.Message}");
            }
        }

        [HttpDelete("groups/{groupId}")]
        public async Task<IActionResult> DeleteSubjectGroup(int groupId)
        {
            try
            {
                _logger.LogInformation("Deleting subject group {GroupId}", groupId);
                var success = await _subjectGroupService.DeleteSubjectGroupAsync(groupId);
                
                if (success)
                {
                    return Ok(new { success = true, message = "Subject group deleted successfully" });
                }
                else
                {
                    return NotFound("Subject group not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject group {GroupId}", groupId);
                return Problem($"Error deleting subject group: {ex.Message}");
            }
        }

        [HttpPost("process/{fileId}")]
        public async Task<IActionResult> ProcessFile(int fileId)
        {
            try
            {
                _logger.LogInformation("🔍 Manually processing file {FileId}", fileId);
                
                var file = await _databaseService.GetFileUploadAsync(fileId);
                if (file == null)
                {
                    _logger.LogWarning("❌ File {FileId} not found", fileId);
                    return NotFound("File not found");
                }

                _logger.LogInformation("📁 Found file: {FileName}, Type: {FileType}, Size: {FileSize}", 
                    file.FileName, file.FileType, file.FileSize);

                var fileProcessingService = HttpContext.RequestServices.GetRequiredService<IFileProcessingService>();
                var contextService = HttpContext.RequestServices.GetRequiredService<ContextService>();
                
                // Process the file
                _logger.LogInformation("⚙️ Starting file processing...");
                var processedFile = await fileProcessingService.ProcessFileAsync(file);
                _logger.LogInformation("📝 Processing completed. Status: {Status}, Text Length: {TextLength}", 
                    processedFile.Status, processedFile.ExtractedText?.Length ?? 0);
                
                // Update processing status
                await _databaseService.UpdateFileProcessingStatusAsync(fileId, processedFile.Status, processedFile.ExtractedText);
                _logger.LogInformation("💾 Database updated with processing status");
                
                // Run auto-detection if successful
                if (processedFile.Status == "completed" && !string.IsNullOrEmpty(processedFile.ExtractedText))
                {
                    _logger.LogInformation("🔍 Running auto-detection on extracted text...");
                    
                    var detectedSubject = contextService.DetectSubjectFromContent(processedFile.ExtractedText);
                    var detectedTopic = contextService.DetectTopicFromContent(processedFile.ExtractedText);
                    
                    _logger.LogInformation("🎯 Detected subject: '{Subject}', topic: '{Topic}'", detectedSubject, detectedTopic);
                    
                    var updateResult = await _databaseService.UpdateFileAutoDetectionAsync(fileId, detectedSubject, detectedTopic);
                    _logger.LogInformation("💾 Auto-detection results saved to database. Update successful: {Success}", updateResult);
                }
                else
                {
                    _logger.LogWarning("⚠️ Skipping auto-detection. Status: {Status}, HasText: {HasText}", 
                        processedFile.Status, !string.IsNullOrEmpty(processedFile.ExtractedText));
                }
                
                return Ok(new { 
                    success = true, 
                    message = "File processed successfully",
                    status = processedFile.Status,
                    extractedTextLength = processedFile.ExtractedText?.Length ?? 0,
                    detectedSubject = processedFile.Status == "completed" ? contextService.DetectSubjectFromContent(processedFile.ExtractedText ?? "") : null,
                    detectedTopic = processedFile.Status == "completed" ? contextService.DetectTopicFromContent(processedFile.ExtractedText ?? "") : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing file {FileId}", fileId);
                return Problem($"Error processing file: {ex.Message}");
            }
        }
    }
}
