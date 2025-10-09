using System.ComponentModel.DataAnnotations;

namespace StudentStudyAI.Models
{
    public class StudyGuideRequest
    {
        [Required]
        public string UserPrompt { get; set; } = string.Empty;
        
        public int? FileId { get; set; }
        public string? Subject { get; set; }
        public string? Topic { get; set; }
    }

    public class QuizRequest
    {
        [Required]
        public string UserPrompt { get; set; } = string.Empty;
        
        public int? FileId { get; set; }
        public string? Subject { get; set; }
        public string? Topic { get; set; }
    }

    public class AnalysisRequest
    {
        [Required]
        public int FileId { get; set; }
        
        public string? Subject { get; set; }
        public string? Topic { get; set; }
    }

    public class ChatRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public int? FileId { get; set; }
        public string? Subject { get; set; }
        public string? Topic { get; set; }
    }

    public class QuizAttemptRequest
    {
        [Required]
        public int QuizId { get; set; }
        
        [Required]
        public List<QuizAnswer> Answers { get; set; } = new();
    }
}
