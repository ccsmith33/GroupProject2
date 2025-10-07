namespace StudentStudyAI.Models
{
    public class FileUpload
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsProcessed { get; set; } = false;
        public string? ProcessingStatus { get; set; }
    }
}
