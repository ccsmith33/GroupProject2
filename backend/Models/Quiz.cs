using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Topic { get; set; }
        public string? Level { get; set; }
        public string Status { get; set; } = "completed";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // JSON properties for complex data
        public string Questions { get; set; } = "[]";
        public List<QuizQuestion> QuestionsList
        {
            get => JsonSerializer.Deserialize<List<QuizQuestion>>(Questions) ?? new List<QuizQuestion>();
            set => Questions = JsonSerializer.Serialize(value);
        }

        public string SourceFileIds { get; set; } = "[]";
        public List<int> SourceFileIdsList
        {
            get => JsonSerializer.Deserialize<List<int>>(SourceFileIds) ?? new List<int>();
            set => SourceFileIds = JsonSerializer.Serialize(value);
        }

        public string SourceStudyGuideIds { get; set; } = "[]";
        public List<int> SourceStudyGuideIdsList
        {
            get => JsonSerializer.Deserialize<List<int>>(SourceStudyGuideIds) ?? new List<int>();
            set => SourceStudyGuideIds = JsonSerializer.Serialize(value);
        }
    }

    public class QuizQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectAnswerIndex { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
    }

    public class QuizAttempt
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public decimal? Score { get; set; }
        public DateTime CompletedAt { get; set; }

        // JSON property for answers
        public string Answers { get; set; } = "[]";
        public List<QuizAnswer> AnswersList
        {
            get => JsonSerializer.Deserialize<List<QuizAnswer>>(Answers) ?? new List<QuizAnswer>();
            set => Answers = JsonSerializer.Serialize(value);
        }
    }

    public class QuizAnswer
    {
        public int QuestionId { get; set; }
        public int SelectedOptionIndex { get; set; }
        public bool IsCorrect { get; set; }
    }
}
