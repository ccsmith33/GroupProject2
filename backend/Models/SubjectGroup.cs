using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class SubjectGroup
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#3498db";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public User? User { get; set; }
        public List<FileUpload>? Files { get; set; }
    }

    public class FileGroupingRequest
    {
        public int FileId { get; set; }
        public string? UserDefinedSubject { get; set; }
        public string? UserDefinedTopic { get; set; }
        public int? SubjectGroupId { get; set; }
    }

    public class BulkFileGroupingRequest
    {
        public List<int> FileIds { get; set; } = new();
        public string? UserDefinedSubject { get; set; }
        public string? UserDefinedTopic { get; set; }
        public int? SubjectGroupId { get; set; }
    }

    public class SubjectGroupRequest
    {
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#3498db";
    }

    public class GroupedFilesResponse
    {
        public Dictionary<string, List<FileUpload>> FilesBySubject { get; set; } = new();
        public List<SubjectGroup> CustomGroups { get; set; } = new();
        public List<FileUpload> UngroupedFiles { get; set; } = new();
    }
}
