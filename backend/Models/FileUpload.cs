using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class FileUpload
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsProcessed { get; set; } = false;
        public string? ProcessingStatus { get; set; }
        public string? StudentLevel { get; set; }
        public string Status { get; set; } = "uploaded";
        public DateTime? ProcessedAt { get; set; }
        public string? ExtractedContent { get; set; }

        // New context fields
        public string? Content { get; set; }
        public string? ProcessedContent { get; set; }
        public int TokenCount { get; set; } = 0;
        public string? Subject { get; set; }
        public string? Topic { get; set; }

        // JSON property for context tags
        public string ContextTags { get; set; } = "[]";
        public List<string> ContextTagsList
        {
            get => JsonSerializer.Deserialize<List<string>>(ContextTags) ?? new List<string>();
            set => ContextTags = JsonSerializer.Serialize(value);
        }

        // Phase 1: File Grouping fields
        public string? AutoDetectedSubject { get; set; }
        public string? AutoDetectedTopic { get; set; }
        public string? UserDefinedSubject { get; set; }
        public string? UserDefinedTopic { get; set; }
        public bool IsUserModified { get; set; } = false;
        public int? SubjectGroupId { get; set; }

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
