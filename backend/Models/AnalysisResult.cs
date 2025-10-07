using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class AnalysisResult
    {
        public int Id { get; set; }
        public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal OverallScore { get; set; }
        public string Feedback { get; set; } = string.Empty; // JSON string
        public string StudyPlan { get; set; } = string.Empty; // JSON string
        public double Confidence { get; set; }

        // Helper properties for working with the JSON data
        public Feedback? FeedbackData 
        { 
            get => string.IsNullOrEmpty(Feedback) ? null : JsonSerializer.Deserialize<Feedback>(Feedback);
            set => Feedback = value != null ? JsonSerializer.Serialize(value) : string.Empty;
        }

        public StudyPlan? StudyPlanData 
        { 
            get => string.IsNullOrEmpty(StudyPlan) ? null : JsonSerializer.Deserialize<StudyPlan>(StudyPlan);
            set => StudyPlan = value != null ? JsonSerializer.Serialize(value) : string.Empty;
        }
    }

    public class Feedback
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new();
        public List<string> WeakAreas { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class DetailedAnalysis
    {
        public int ConceptUnderstanding { get; set; }
        public int ProblemSolving { get; set; }
        public int Completeness { get; set; }
        public int Accuracy { get; set; }
    }

    public class StudyPlan
    {
        public List<string> PriorityTopics { get; set; } = new();
        public List<string> SuggestedResources { get; set; } = new();
        public string EstimatedTime { get; set; } = string.Empty;
        public List<string> NextSteps { get; set; } = new();
    }
}
