using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class UserSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SessionToken { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        // JSON property for context window
        public string ContextWindow { get; set; } = "{}";
        public ContextWindow? ContextWindowData
        {
            get => JsonSerializer.Deserialize<ContextWindow>(ContextWindow);
            set => ContextWindow = JsonSerializer.Serialize(value ?? new ContextWindow());
        }
    }

    public class ContextWindow
    {
        public List<int> RecentFileIds { get; set; } = new();
        public List<int> RecentStudyGuideIds { get; set; } = new();
        public List<int> RecentConversationIds { get; set; } = new();
        public string? CurrentSubject { get; set; }
        public string? CurrentTopic { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
