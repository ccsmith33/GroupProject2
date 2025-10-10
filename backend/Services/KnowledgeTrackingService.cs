using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public class KnowledgeTrackingService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ContentAnalysisService _contentAnalysisService;
        private readonly ILogger<KnowledgeTrackingService> _logger;

        public KnowledgeTrackingService(IDatabaseService databaseService, ContentAnalysisService contentAnalysisService, ILogger<KnowledgeTrackingService> logger)
        {
            _databaseService = databaseService;
            _contentAnalysisService = contentAnalysisService;
            _logger = logger;
        }

        public async Task<UserKnowledgeProfile> UpdateKnowledgeLevelAsync(int userId, string subject, int newLevel, string changeReason = "content_analysis", decimal? confidenceScore = null)
        {
            try
            {
                _logger.LogInformation("Updating knowledge level for user {UserId} in subject {Subject} to level {Level}", userId, subject, newLevel);

                // Get existing profile
                var existingProfile = await _databaseService.GetUserKnowledgeProfileAsync(userId, subject);
                
                int? previousLevel = existingProfile?.KnowledgeLevel;
                
                // Create or update profile
                var profile = new UserKnowledgeProfile
                {
                    UserId = userId,
                    Subject = subject,
                    KnowledgeLevel = newLevel,
                    ConfidenceScore = confidenceScore ?? 0.75m,
                    LastUpdated = DateTime.UtcNow,
                    IsActive = true
                };

                if (existingProfile != null)
                {
                    profile.Id = existingProfile.Id;
                    profile.CreatedAt = existingProfile.CreatedAt;
                    await _databaseService.UpdateUserKnowledgeProfileAsync(profile);
                }
                else
                {
                    profile.CreatedAt = DateTime.UtcNow;
                    var profileId = await _databaseService.CreateUserKnowledgeProfileAsync(profile);
                    profile.Id = profileId;
                }

                // Record progression
                if (previousLevel.HasValue && previousLevel.Value != newLevel)
                {
                    var progression = new KnowledgeProgression
                    {
                        UserId = userId,
                        Subject = subject,
                        PreviousLevel = previousLevel.Value,
                        NewLevel = newLevel,
                        ChangeReason = changeReason,
                        ConfidenceScore = confidenceScore,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _databaseService.CreateKnowledgeProgressionAsync(progression);
                }

                _logger.LogInformation("Successfully updated knowledge level for user {UserId} in subject {Subject}", userId, subject);
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating knowledge level for user {UserId} in subject {Subject}", userId, subject);
                throw;
            }
        }

        public async Task<QuizPerformance> RecordQuizPerformanceAsync(int userId, int quizId, decimal score, int? timeSpent = null)
        {
            try
            {
                _logger.LogInformation("Recording quiz performance for user {UserId}, quiz {QuizId}, score {Score}", userId, quizId, score);

                // Get quiz details to determine knowledge level
                var quiz = await _databaseService.GetQuizAsync(quizId);
                var knowledgeLevel = quiz?.Level ?? "High School";

                var performance = new QuizPerformance
                {
                    UserId = userId,
                    QuizId = quizId,
                    Score = score,
                    KnowledgeLevel = GetKnowledgeLevelFromString(knowledgeLevel),
                    Difficulty = CalculateDifficultyFromScore(score),
                    TimeSpent = timeSpent,
                    CompletedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                var performanceId = await _databaseService.CreateQuizPerformanceAsync(performance);
                performance.Id = performanceId;

                // Update user's knowledge level based on performance
                if (quiz?.Subject != null)
                {
                    await UpdateKnowledgeLevelFromPerformanceAsync(userId, quiz.Subject, score, performance.KnowledgeLevel ?? 3);
                }

                _logger.LogInformation("Successfully recorded quiz performance for user {UserId}", userId);
                return performance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording quiz performance for user {UserId}", userId);
                throw;
            }
        }

        public async Task<KnowledgeAnalytics> GetKnowledgeAnalyticsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting knowledge analytics for user {UserId}", userId);

                // Get data with null safety
                var profiles = await _databaseService.GetUserKnowledgeProfilesAsync(userId) ?? new List<UserKnowledgeProfile>();
                var recentPerformance = await _databaseService.GetRecentQuizPerformanceAsync(userId, 10) ?? new List<QuizPerformance>();
                var progressionHistory = await _databaseService.GetKnowledgeProgressionAsync(userId) ?? new List<KnowledgeProgression>();
                var preferences = await _databaseService.GetUserLearningPreferencesAsync(userId);

                var analytics = new KnowledgeAnalytics
                {
                    SubjectProfiles = profiles.ToDictionary(p => p.Subject, p => p),
                    RecentPerformance = recentPerformance,
                    ProgressionHistory = progressionHistory,
                    Preferences = preferences
                };

                // Calculate subject mastery scores
                foreach (var profile in profiles)
                {
                    var masteryScore = CalculateSubjectMastery(profile, recentPerformance.Where(p => p.Quiz?.Subject == profile.Subject));
                    analytics.SubjectMastery[profile.Subject] = masteryScore;
                }

                // Generate recommendations
                analytics.RecommendedSubjects = GenerateRecommendations(analytics);

                _logger.LogInformation("Successfully generated knowledge analytics for user {UserId}", userId);
                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting knowledge analytics for user {UserId}: {ErrorMessage}", userId, ex.Message);
                
                // Return empty analytics instead of throwing
                return new KnowledgeAnalytics
                {
                    SubjectProfiles = new Dictionary<string, UserKnowledgeProfile>(),
                    RecentPerformance = new List<QuizPerformance>(),
                    ProgressionHistory = new List<KnowledgeProgression>(),
                    Preferences = null,
                    SubjectMastery = new Dictionary<string, decimal>(),
                    RecommendedSubjects = new List<string>()
                };
            }
        }

        public async Task<UserLearningPreferences> UpdateLearningPreferencesAsync(int userId, LearningPreferencesRequest request)
        {
            try
            {
                _logger.LogInformation("Updating learning preferences for user {UserId}", userId);

                var preferences = new UserLearningPreferences
                {
                    UserId = userId,
                    PreferredQuizLength = request.PreferredQuizLength,
                    CustomQuestionMultiplier = request.CustomQuestionMultiplier ?? 1.0m,
                    PreferredDifficulty = request.PreferredDifficulty,
                    TimeAvailable = request.TimeAvailable ?? 30,
                    StudyStyle = request.StudyStyle,
                    UpdatedAt = DateTime.UtcNow
                };

                var existing = await _databaseService.GetUserLearningPreferencesAsync(userId);
                if (existing != null)
                {
                    preferences.Id = existing.Id;
                    preferences.CreatedAt = existing.CreatedAt;
                    await _databaseService.UpdateUserLearningPreferencesAsync(preferences);
                }
                else
                {
                    preferences.CreatedAt = DateTime.UtcNow;
                    var preferencesId = await _databaseService.CreateUserLearningPreferencesAsync(preferences);
                    preferences.Id = preferencesId;
                }

                _logger.LogInformation("Successfully updated learning preferences for user {UserId}", userId);
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning preferences for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ContentDifficultyAnalysis> AnalyzeContentDifficultyAsync(int fileId)
        {
            try
            {
                _logger.LogInformation("Analyzing content difficulty for file {FileId}", fileId);

                var file = await _databaseService.GetFileUploadAsync(fileId);
                if (file == null)
                {
                    throw new ArgumentException("File not found");
                }

                var analysis = _contentAnalysisService.AnalyzeContent(new List<FileUpload> { file });
                
                var difficultyAnalysis = new ContentDifficultyAnalysis
                {
                    FileId = fileId,
                    ComplexityScore = (decimal)analysis.ComplexityScore,
                    KnowledgeLevel = (int)analysis.KnowledgeLevel,
                    UniqueConcepts = analysis.UniqueConcepts,
                    ContentVolume = analysis.ContentVolume,
                    EstimatedQuestions = analysis.EstimatedQuestions,
                    TimeEstimate = analysis.TimeEstimate,
                    AnalyzedAt = DateTime.UtcNow
                };

                var analysisId = await _databaseService.CreateContentDifficultyAnalysisAsync(difficultyAnalysis);
                difficultyAnalysis.Id = analysisId;

                _logger.LogInformation("Successfully analyzed content difficulty for file {FileId}", fileId);
                return difficultyAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing content difficulty for file {FileId}", fileId);
                throw;
            }
        }

        private async Task UpdateKnowledgeLevelFromPerformanceAsync(int userId, string subject, decimal score, int currentLevel)
        {
            // Adjust knowledge level based on quiz performance
            int newLevel = currentLevel;
            
            if (score >= 90)
            {
                newLevel = Math.Min(6, currentLevel + 1); // Move up one level
            }
            else if (score < 60)
            {
                newLevel = Math.Max(1, currentLevel - 1); // Move down one level
            }

            if (newLevel != currentLevel)
            {
                await UpdateKnowledgeLevelAsync(userId, subject, newLevel, "quiz_performance", (decimal)(score / 100.0m));
            }
        }

        private int GetKnowledgeLevelFromString(string level)
        {
            return level.ToLower() switch
            {
                "elementary" => 1,
                "middle school" => 2,
                "high school" => 3,
                "college" => 4,
                "graduate" => 5,
                "expert" => 6,
                _ => 3
            };
        }

        private int CalculateDifficultyFromScore(decimal score)
        {
            return score switch
            {
                >= 90 => 5, // Very Easy
                >= 80 => 4, // Easy
                >= 70 => 3, // Medium
                >= 60 => 2, // Hard
                _ => 1      // Very Hard
            };
        }

        private decimal CalculateSubjectMastery(UserKnowledgeProfile profile, IEnumerable<QuizPerformance> performances)
        {
            if (!performances.Any())
            {
                return profile.ConfidenceScore;
            }

            var avgScore = performances.Average(p => p.Score ?? 0);
            var levelWeight = profile.KnowledgeLevel / 6.0m;
            var scoreWeight = avgScore / 100.0m;

            return (levelWeight * 0.6m) + (scoreWeight * 0.4m);
        }

        private List<string> GenerateRecommendations(KnowledgeAnalytics analytics)
        {
            var recommendations = new List<string>();

            // Find subjects with low mastery
            var lowMasterySubjects = analytics.SubjectMastery
                .Where(kvp => kvp.Value < 0.6m)
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .Take(3);

            recommendations.AddRange(lowMasterySubjects);

            // Find subjects not yet studied
            var studiedSubjects = analytics.SubjectProfiles.Keys.ToHashSet();
            var allSubjects = new[] { "Mathematics", "Science", "English", "History", "Computer Science" };
            var unstudiedSubjects = allSubjects.Except(studiedSubjects).Take(2);

            recommendations.AddRange(unstudiedSubjects);

            return recommendations.Distinct().ToList();
        }

        public async Task<UserLearningPreferences?> GetUserLearningPreferencesAsync(int userId)
        {
            return await _databaseService.GetUserLearningPreferencesAsync(userId);
        }

        public async Task<List<KnowledgeProgression>> GetKnowledgeProgressionAsync(int userId, string? subject = null)
        {
            return await _databaseService.GetKnowledgeProgressionAsync(userId, subject);
        }
    }
}
