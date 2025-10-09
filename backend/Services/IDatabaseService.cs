using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public interface IDatabaseService
    {
        Task<int> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateUserLastLoginAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<int> CreateFileUploadAsync(FileUpload fileUpload);
        Task<FileUpload?> GetFileByIdAsync(int id);
        Task<FileUpload?> GetFileUploadAsync(int fileId);
        Task<List<FileUpload>> GetFileUploadsByUserIdAsync(int userId);
        Task<List<FileUpload>> GetFilesByUserIdAsync(int userId);
        Task<bool> DeleteFileUploadAsync(int fileId);
        Task<bool> UpdateFileProcessingStatusAsync(int fileId, string status, string? extractedContent = null);
        Task<int> CreateAnalysisResultAsync(AnalysisResult analysisResult);
        Task<List<AnalysisResult>> GetAnalysisResultsByUserIdAsync(int userId);
        Task<int> CreateStudyGuideAsync(StudyGuide studyGuide);
        Task<List<StudyGuide>> GetStudyGuidesByUserIdAsync(int userId);
        Task<int> CreateQuizAsync(Quiz quiz);
        Task<List<Quiz>> GetQuizzesByUserIdAsync(int userId);
        Task<int> CreateQuizAttemptAsync(QuizAttempt quizAttempt);
        Task<int> CreateStudySessionAsync(StudySession session);
        Task<List<StudySession>> GetStudySessionsByUserIdAsync(int userId);
        Task<StudySession?> GetStudySessionByIdAsync(int id);
        Task<bool> UpdateStudySessionAsync(StudySession session);
        Task<bool> DeleteStudySessionAsync(int id);
        Task<int> CreateConversationAsync(Conversation conversation);
        Task<List<Conversation>> GetConversationsByUserIdAsync(int userId, int limit = 10);
        Task<int> CreateUserSessionAsync(UserSession session);
        Task<UserSession?> GetUserSessionByTokenAsync(string sessionToken);
        Task UpdateUserSessionAsync(UserSession session);
        Task<List<FileUpload>> GetRelevantFilesForContextAsync(int userId, string? subject = null, string? topic = null, int limit = 5);
        Task InitializeDatabaseAsync();
        Task UpdateDatabaseSchemaAsync();

        // Phase 1: File Grouping methods
        Task<int> CreateSubjectGroupAsync(SubjectGroup subjectGroup);
        Task<SubjectGroup?> GetSubjectGroupAsync(int id);
        Task<List<SubjectGroup>> GetSubjectGroupsByUserIdAsync(int userId);
        Task<bool> UpdateSubjectGroupAsync(SubjectGroup subjectGroup);
        Task<bool> DeleteSubjectGroupAsync(int id);
        Task<bool> UpdateFileUploadAsync(FileUpload fileUpload);
        Task<bool> UpdateFileAutoDetectionAsync(int fileId, string? autoDetectedSubject, string? autoDetectedTopic);
        Task<GroupedFilesResponse> GetGroupedFilesAsync(int userId);

        // Phase 3: Knowledge Tracking methods
        Task<int> CreateUserKnowledgeProfileAsync(UserKnowledgeProfile profile);
        Task<UserKnowledgeProfile?> GetUserKnowledgeProfileAsync(int userId, string subject);
        Task<List<UserKnowledgeProfile>> GetUserKnowledgeProfilesAsync(int userId);
        Task<bool> UpdateUserKnowledgeProfileAsync(UserKnowledgeProfile profile);
        Task<int> CreateQuizPerformanceAsync(QuizPerformance performance);
        Task<List<QuizPerformance>> GetRecentQuizPerformanceAsync(int userId, int limit = 10);
        Task<int> CreateKnowledgeProgressionAsync(KnowledgeProgression progression);
        Task<List<KnowledgeProgression>> GetKnowledgeProgressionAsync(int userId, string? subject = null);
        Task<UserLearningPreferences?> GetUserLearningPreferencesAsync(int userId);
        Task<int> CreateUserLearningPreferencesAsync(UserLearningPreferences preferences);
        Task<bool> UpdateUserLearningPreferencesAsync(UserLearningPreferences preferences);
        Task<int> CreateContentDifficultyAnalysisAsync(ContentDifficultyAnalysis analysis);
        Task<Quiz?> GetQuizAsync(int id);
    }
}
