namespace StudentStudyAI.Models
{
    public class FileAnalysis
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public List<string> KeyPoints { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "completed";
    }
}
