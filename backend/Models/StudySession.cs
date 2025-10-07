using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class StudySession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? TimeSpan.Zero;
        public string? Notes { get; set; }
        public string FileIdsJson { get; set; } = "[]"; // JSON string for database storage
        public bool IsActive { get; set; } = true;

        // Helper property for working with FileIds
        public List<int> FileIds 
        { 
            get => string.IsNullOrEmpty(FileIdsJson) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(FileIdsJson) ?? new List<int>();
            set => FileIdsJson = JsonSerializer.Serialize(value);
        }
    }
}
