using Microsoft.AspNetCore.Mvc;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using MySql.Data.MySqlClient;

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
        private readonly FileValidator _fileValidator;
        private readonly ILogger<FileGroupingController> _logger;

        public FileGroupingController(
            SubjectGroupService subjectGroupService,
            IFileStorageService fileStorageService,
            IFileProcessingService fileProcessingService,
            IDatabaseService databaseService,
            FileValidator fileValidator,
            ILogger<FileGroupingController> logger)
        {
            _subjectGroupService = subjectGroupService;
            _fileStorageService = fileStorageService;
            _fileProcessingService = fileProcessingService;
            _databaseService = databaseService;
            _fileValidator = fileValidator;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string userId, [FromForm] string subject = "", [FromForm] string studentLevel = "")
        {
            try
            {
                // Handle guest users - use ID 0 for guests
                int userIdInt;
                if (userId.ToLower() == "guest" || string.IsNullOrEmpty(userId))
                {
                    userIdInt = 1; // Guest user ID
                    
                    // Ensure guest user exists in database
                    var existingGuest = await _databaseService.GetUserByIdAsync(1);
                    if (existingGuest == null)
                    {
                        _logger.LogInformation("Guest user not found, creating guest user with ID 1");
                        // Create guest user directly with ID 1
                        using var connection = await ((DatabaseService)_databaseService).GetConnectionAsync();
                        
                        // First try to insert with ID 1
                        var command = new MySqlCommand(@"
                            INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt, LastLoginAt, IsActive)
                            VALUES (1, 'guest', 'guest@system.local', '', NOW(), NOW(), TRUE)
                        ", connection);
                        
                        try
                        {
                            await command.ExecuteNonQueryAsync();
                            _logger.LogInformation("Guest user inserted with ID 1");
                        }
                        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate key error
                        {
                            _logger.LogInformation("Guest user already exists, continuing");
                        }
                        catch (MySqlException ex) when (ex.Number == 1364) // Field doesn't have a default value
                        {
                            _logger.LogWarning("Cannot insert with ID 0, trying without specifying ID");
                            // Try without specifying ID and let it auto-increment
                            command = new MySqlCommand(@"
                                INSERT INTO Users (Username, Email, PasswordHash, CreatedAt, LastLoginAt, IsActive)
                                VALUES ('guest', 'guest@system.local', '', NOW(), NOW(), TRUE)
                            ", connection);
                            await command.ExecuteNonQueryAsync();
                            _logger.LogInformation("Guest user inserted with auto-increment ID");
                        }
                        _logger.LogInformation("Guest user creation command executed");
                        
                        // Verify the guest user was created
                        var verifyGuest = await _databaseService.GetUserByIdAsync(1);
                        if (verifyGuest == null)
                        {
                            _logger.LogError("Failed to create guest user - still null after creation attempt");
                        }
                        else
                        {
                            _logger.LogInformation("Guest user successfully created and verified");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Guest user already exists");
                    }
                }
                else if (!int.TryParse(userId, out userIdInt))
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a valid number or 'guest'" });
                }

                // Validate file using FileValidator
                var validationResult = _fileValidator.ValidateFile(file, userIdInt);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { 
                        error = "File validation failed", 
                        message = validationResult.ErrorMessage 
                    });
                }

                // Additional content validation
                if (!await _fileValidator.ValidateFileContent(file))
                {
                    return BadRequest(new { 
                        error = "File content validation failed", 
                        message = "File content appears to be unsafe or malicious" 
                    });
                }

                // Save file to storage using sanitized filename
                var filePath = await _fileStorageService.SaveFileAsync(file, userIdInt);
                var fileExtension = Path.GetExtension(validationResult.SanitizedFileName).ToLowerInvariant();

                // Create file upload record
                var fileUpload = new FileUpload
                {
                    UserId = userIdInt,
                    FileName = validationResult.SanitizedFileName, // Use sanitized filename
                    OriginalFileName = validationResult.OriginalFileName, // Store original for display
                    FilePath = filePath,
                    FileType = fileExtension,
                    FileSize = validationResult.FileSize,
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
                var backgroundJobService = HttpContext.RequestServices.GetRequiredService<BackgroundJobService>();
                backgroundJobService.EnqueueJob(processingJob);

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

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                _logger.LogInformation("Soft deleting file {FileId}", fileId);
                
                var file = await _databaseService.GetFileUploadAsync(fileId);
                if (file == null)
                {
                    return NotFound("File not found");
                }

                // Soft delete - mark as deleted but keep the file
                file.IsDeleted = true;
                file.DeletedAt = DateTime.UtcNow;
                
                var success = await _databaseService.UpdateFileUploadAsync(file);
                if (success)
                {
                    return Ok(new { success = true, message = "File moved to trash" });
                }
                else
                {
                    return Problem("Failed to delete file");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return Problem($"Error deleting file: {ex.Message}");
            }
        }

        [HttpPost("{fileId}/restore")]
        public async Task<IActionResult> RestoreFile(int fileId)
        {
            try
            {
                _logger.LogInformation("Restoring file {FileId}", fileId);
                
                var file = await _databaseService.GetFileUploadAsync(fileId);
                if (file == null)
                {
                    return NotFound("File not found");
                }

                // Restore file
                file.IsDeleted = false;
                file.DeletedAt = null;
                
                var success = await _databaseService.UpdateFileUploadAsync(file);
                if (success)
                {
                    return Ok(new { success = true, message = "File restored successfully" });
                }
                else
                {
                    return Problem("Failed to restore file");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring file {FileId}", fileId);
                return Problem($"Error restoring file: {ex.Message}");
            }
        }

        [HttpGet("{fileId}/download")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                _logger.LogInformation("Downloading file {FileId}", fileId);
                
                var file = await _databaseService.GetFileUploadAsync(fileId);
                if (file == null)
                {
                    _logger.LogWarning("File {FileId} not found", fileId);
                    return NotFound("File not found");
                }

                var filePath = file.FilePath;
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("File {FileId} not found on server at path: {FilePath}", fileId, filePath);
                    return NotFound("File not found on server");
                }

                var fileStream = System.IO.File.OpenRead(filePath);
                var contentType = _fileStorageService.GetContentType(System.IO.Path.GetExtension(file.FileName));
                
                _logger.LogInformation("Successfully serving file {FileId}: {FileName}", fileId, file.FileName);
                return File(fileStream, contentType, file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileId}", fileId);
                return Problem($"Error downloading file: {ex.Message}");
            }
        }

        [HttpPost("reorganize/{userId}")]
        public async Task<IActionResult> ReorganizeFiles(int userId)
        {
            try
            {
                _logger.LogInformation("Reorganizing files for user {UserId}", userId);
                
                // Get all files for the user
                var files = await _databaseService.GetFileUploadsByUserIdAsync(userId);
                var contextService = HttpContext.RequestServices.GetRequiredService<ContextService>();
                
                int reorganizedCount = 0;
                
                foreach (var file in files.Where(f => !f.IsDeleted && !string.IsNullOrEmpty(f.ExtractedContent)))
                {
                    try
                    {
                        // Re-detect subject and topic
                        var detectedSubject = contextService.DetectSubjectFromContent(file.ExtractedContent);
                        var detectedTopic = contextService.DetectTopicFromContent(file.ExtractedContent);
                        
                        // Update file with new detection
                        var updateResult = await _databaseService.UpdateFileAutoDetectionAsync(file.Id, detectedSubject, detectedTopic);
                        if (updateResult)
                        {
                            reorganizedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to reorganize file {FileId}", file.Id);
                    }
                }
                
                _logger.LogInformation("Reorganized {Count} files for user {UserId}", reorganizedCount, userId);
                
                return Ok(new { 
                    success = true, 
                    message = $"Reorganized {reorganizedCount} files successfully",
                    reorganizedCount = reorganizedCount,
                    totalFiles = files.Count(f => !f.IsDeleted)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reorganizing files for user {UserId}", userId);
                return Problem($"Error reorganizing files: {ex.Message}");
            }
        }
    }
}
