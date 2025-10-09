using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class StudyGuide
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Topic { get; set; }
        public string? Level { get; set; }
        public List<string> KeyPoints { get; set; } = new();
        public string? Summary { get; set; }
        public string Status { get; set; } = "completed";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // JSON properties for complex data
        public string SourceFileIds { get; set; } = "[]";
        public List<int> SourceFileIdsList
        {
            get => JsonSerializer.Deserialize<List<int>>(SourceFileIds) ?? new List<int>();
            set => SourceFileIds = JsonSerializer.Serialize(value);
        }
    }
}
