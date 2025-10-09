using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public class SubjectGroupService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<SubjectGroupService> _logger;

        public SubjectGroupService(IDatabaseService databaseService, ILogger<SubjectGroupService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<SubjectGroup> CreateSubjectGroupAsync(int userId, SubjectGroupRequest request)
        {
            try
            {
                _logger.LogInformation("Creating subject group '{GroupName}' for user {UserId}", request.GroupName, userId);

                var subjectGroup = new SubjectGroup
                {
                    UserId = userId,
                    GroupName = request.GroupName,
                    Description = request.Description,
                    Color = request.Color,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var groupId = await _databaseService.CreateSubjectGroupAsync(subjectGroup);
                subjectGroup.Id = groupId;

                _logger.LogInformation("Successfully created subject group {GroupId}", groupId);
                return subjectGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject group for user {UserId}", userId);
                throw;
            }
        }

        public async Task<SubjectGroup?> GetSubjectGroupAsync(int groupId)
        {
            try
            {
                return await _databaseService.GetSubjectGroupAsync(groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subject group {GroupId}", groupId);
                throw;
            }
        }

        public async Task<List<SubjectGroup>> GetSubjectGroupsByUserIdAsync(int userId)
        {
            try
            {
                return await _databaseService.GetSubjectGroupsByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subject groups for user {UserId}", userId);
                throw;
            }
        }

        public async Task<SubjectGroup?> UpdateSubjectGroupAsync(int groupId, SubjectGroupRequest request)
        {
            try
            {
                _logger.LogInformation("Updating subject group {GroupId}", groupId);

                var existingGroup = await _databaseService.GetSubjectGroupAsync(groupId);
                if (existingGroup == null)
                {
                    return null;
                }

                existingGroup.GroupName = request.GroupName;
                existingGroup.Description = request.Description;
                existingGroup.Color = request.Color;
                existingGroup.UpdatedAt = DateTime.UtcNow;

                var success = await _databaseService.UpdateSubjectGroupAsync(existingGroup);
                if (success)
                {
                    _logger.LogInformation("Successfully updated subject group {GroupId}", groupId);
                    return existingGroup!;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject group {GroupId}", groupId);
                throw;
            }
        }

        public async Task<bool> DeleteSubjectGroupAsync(int groupId)
        {
            try
            {
                _logger.LogInformation("Deleting subject group {GroupId}", groupId);

                var success = await _databaseService.DeleteSubjectGroupAsync(groupId);
                if (success)
                {
                    _logger.LogInformation("Successfully deleted subject group {GroupId}", groupId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject group {GroupId}", groupId);
                throw;
            }
        }

        public async Task<GroupedFilesResponse> GetGroupedFilesAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting grouped files for user {UserId}", userId);

                var files = await _databaseService.GetFileUploadsByUserIdAsync(userId);
                var customGroups = await _databaseService.GetSubjectGroupsByUserIdAsync(userId);

                _logger.LogInformation("Retrieved {FileCount} files and {GroupCount} groups for user {UserId}", 
                    files?.Count ?? 0, customGroups?.Count ?? 0, userId);

                var response = new GroupedFilesResponse
                {
                    CustomGroups = customGroups,
                    UngroupedFiles = new List<FileUpload>()
                };

                // Group files by their effective subject (user-defined or auto-detected)
                var filesBySubject = new Dictionary<string, List<FileUpload>>();

                foreach (var file in files)
                {
                    var effectiveSubject = file.UserDefinedSubject ?? file.AutoDetectedSubject ?? "Ungrouped";
                    
                    if (effectiveSubject == "Ungrouped")
                    {
                        response.UngroupedFiles.Add(file);
                    }
                    else
                    {
                        if (!filesBySubject.ContainsKey(effectiveSubject))
                        {
                            filesBySubject[effectiveSubject] = new List<FileUpload>();
                        }
                        filesBySubject[effectiveSubject].Add(file);
                    }
                }

                response.FilesBySubject = filesBySubject;

                _logger.LogInformation("Found {SubjectCount} subject groups and {UngroupedCount} ungrouped files", 
                    filesBySubject.Count, response.UngroupedFiles.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grouped files for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateFileGroupingAsync(int fileId, FileGroupingRequest request)
        {
            try
            {
                _logger.LogInformation("Updating file grouping for file {FileId}", fileId);

                var file = await _databaseService.GetFileUploadAsync(fileId);
                if (file == null)
                {
                    return false;
                }

                // Update user-defined values
                file.UserDefinedSubject = request.UserDefinedSubject;
                file.UserDefinedTopic = request.UserDefinedTopic;
                file.SubjectGroupId = request.SubjectGroupId;
                file.IsUserModified = true;

                var success = await _databaseService.UpdateFileUploadAsync(file);
                if (success)
                {
                    _logger.LogInformation("Successfully updated file grouping for file {FileId}", fileId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating file grouping for file {FileId}", fileId);
                throw;
            }
        }

        public async Task<bool> BulkUpdateFileGroupingAsync(int userId, BulkFileGroupingRequest request)
        {
            try
            {
                _logger.LogInformation("Bulk updating file grouping for {FileCount} files", request.FileIds.Count);

                var success = true;
                foreach (var fileId in request.FileIds)
                {
                    var fileRequest = new FileGroupingRequest
                    {
                        FileId = fileId,
                        UserDefinedSubject = request.UserDefinedSubject,
                        UserDefinedTopic = request.UserDefinedTopic,
                        SubjectGroupId = request.SubjectGroupId
                    };

                    var fileSuccess = await UpdateFileGroupingAsync(fileId, fileRequest);
                    if (!fileSuccess)
                    {
                        success = false;
                    }
                }

                if (success)
                {
                    _logger.LogInformation("Successfully bulk updated file grouping");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating file grouping for user {UserId}", userId);
                throw;
            }
        }
    }
}
