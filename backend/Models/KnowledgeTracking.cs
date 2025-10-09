using System.Text.Json;

namespace StudentStudyAI.Models
{
    public class UserKnowledgeProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public int KnowledgeLevel { get; set; }
        public decimal ConfidenceScore { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public User? User { get; set; }
    }

    public class QuizPerformance
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public decimal? Score { get; set; }
        public int? KnowledgeLevel { get; set; }
        public int? Difficulty { get; set; }
        public int? TimeSpent { get; set; } // seconds
        public DateTime CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public User? User { get; set; }
        public Quiz? Quiz { get; set; }
    }

    public class KnowledgeProgression
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public int? PreviousLevel { get; set; }
        public int NewLevel { get; set; }
        public string ChangeReason { get; set; } = string.Empty;
        public decimal? ConfidenceScore { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public User? User { get; set; }
    }

    public class UserLearningPreferences
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PreferredQuizLength { get; set; } = "standard";
        public decimal CustomQuestionMultiplier { get; set; } = 1.0m;
        public string PreferredDifficulty { get; set; } = "adaptive";
        public int TimeAvailable { get; set; } = 30; // minutes
        public string StudyStyle { get; set; } = "balanced";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public User? User { get; set; }
    }

    public class ContentDifficultyAnalysis
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public decimal ComplexityScore { get; set; }
        public int KnowledgeLevel { get; set; }
        public int UniqueConcepts { get; set; }
        public int ContentVolume { get; set; }
        public int EstimatedQuestions { get; set; }
        public int TimeEstimate { get; set; } // minutes
        public DateTime AnalyzedAt { get; set; }
        
        // Navigation properties
        public FileUpload? File { get; set; }
    }

    public class KnowledgeTrackingRequest
    {
        public int UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public int? NewLevel { get; set; }
        public string? ChangeReason { get; set; }
        public decimal? ConfidenceScore { get; set; }
    }

    public class LearningPreferencesRequest
    {
        public string PreferredQuizLength { get; set; } = "standard";
        public decimal? CustomQuestionMultiplier { get; set; }
        public string PreferredDifficulty { get; set; } = "adaptive";
        public int? TimeAvailable { get; set; }
        public string StudyStyle { get; set; } = "balanced";
    }

    public class KnowledgeAnalytics
    {
        public Dictionary<string, UserKnowledgeProfile> SubjectProfiles { get; set; } = new();
        public List<QuizPerformance> RecentPerformance { get; set; } = new();
        public List<KnowledgeProgression> ProgressionHistory { get; set; } = new();
        public UserLearningPreferences? Preferences { get; set; }
        public Dictionary<string, decimal> SubjectMastery { get; set; } = new();
        public List<string> RecommendedSubjects { get; set; } = new();
    }
}
