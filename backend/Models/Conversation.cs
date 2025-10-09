using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Topic { get; set; }
        public DateTime CreatedAt { get; set; }

        // JSON properties for complex data
        public string ContextFileIds { get; set; } = "[]";
        public List<int> ContextFileIdsList
        {
            get => JsonSerializer.Deserialize<List<int>>(ContextFileIds) ?? new List<int>();
            set => ContextFileIds = JsonSerializer.Serialize(value);
        }

        public string ContextStudyGuideIds { get; set; } = "[]";
        public List<int> ContextStudyGuideIdsList
        {
            get => JsonSerializer.Deserialize<List<int>>(ContextStudyGuideIds) ?? new List<int>();
            set => ContextStudyGuideIds = JsonSerializer.Serialize(value);
        }
    }
}
