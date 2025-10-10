using MySql.Data.MySqlClient;
using StudentStudyAI.Models;
using System.Data;

namespace StudentStudyAI.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("DefaultConnection string is missing from appsettings.json. Please add your MySQL connection string.");
            _logger = logger;
        }

        public async Task<MySqlConnection> GetConnectionAsync()
        {
            try
            {
                var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                return connection;
            }
            catch (MySqlException ex) when (ex.Number == 1042)
            {
                throw new InvalidOperationException("Cannot connect to MySQL server. Please ensure MySQL is running and the connection string is correct.", ex);
            }
            catch (MySqlException ex) when (ex.Number == 1045)
            {
                throw new InvalidOperationException("MySQL authentication failed. Please check your username and password in appsettings.json.", ex);
            }
            catch (MySqlException ex) when (ex.Number == 1049)
            {
                throw new InvalidOperationException("Database 'StudentStudyAI' does not exist. Please run the /init-db endpoint to create it.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database connection failed: {ex.Message}", ex);
            }
        }

        // User operations
        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM Users WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32("Id"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    LastLoginAt = reader.GetDateTime("LastLoginAt"),
                    IsActive = reader.GetBoolean("IsActive")
                };
            }
            return null;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM Users WHERE Username = @username", connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32("Id"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    LastLoginAt = reader.GetDateTime("LastLoginAt"),
                    IsActive = reader.GetBoolean("IsActive")
                };
            }
            return null;
        }

        public async Task<int> CreateUserAsync(User user)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO Users (Username, Email, PasswordHash, CreatedAt, LastLoginAt, IsActive) " +
                "VALUES (@username, @email, @passwordHash, @createdAt, @lastLoginAt, @isActive); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@createdAt", user.CreatedAt);
            command.Parameters.AddWithValue("@lastLoginAt", user.LastLoginAt);
            command.Parameters.AddWithValue("@isActive", user.IsActive);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // File operations
        public async Task<FileUpload?> GetFileByIdAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM FileUploads WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new FileUpload
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    FileName = reader.GetString("FileName"),
                    FilePath = reader.GetString("FilePath"),
                    FileType = reader.GetString("FileType"),
                    FileSize = reader.GetInt64("FileSize"),
                    UploadedAt = reader.GetDateTime("UploadedAt"),
                    IsProcessed = reader.GetBoolean("IsProcessed"),
                    ProcessingStatus = reader.IsDBNull("ProcessingStatus") ? null : reader.GetString("ProcessingStatus"),
                    Content = reader.IsDBNull("Content") ? null : reader.GetString("Content"),
                    ProcessedContent = reader.IsDBNull("ProcessedContent") ? null : reader.GetString("ProcessedContent"),
                    TokenCount = reader.GetInt32("TokenCount"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    ContextTags = reader.IsDBNull("ContextTags") ? "[]" : reader.GetString("ContextTags")
                };
            }
            return null;
        }

        public async Task<List<FileUpload>> GetFilesByUserIdAsync(int userId)
        {
            var files = new List<FileUpload>();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM FileUploads WHERE UserId = @userId ORDER BY UploadedAt DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                files.Add(new FileUpload
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    FileName = reader.GetString("FileName"),
                    FilePath = reader.GetString("FilePath"),
                    FileType = reader.GetString("FileType"),
                    FileSize = reader.GetInt64("FileSize"),
                    UploadedAt = reader.GetDateTime("UploadedAt"),
                    IsProcessed = reader.GetBoolean("IsProcessed"),
                    ProcessingStatus = reader.IsDBNull("ProcessingStatus") ? null : reader.GetString("ProcessingStatus"),
                    Content = reader.IsDBNull("Content") ? null : reader.GetString("Content"),
                    ProcessedContent = reader.IsDBNull("ProcessedContent") ? null : reader.GetString("ProcessedContent"),
                    TokenCount = reader.GetInt32("TokenCount"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    ContextTags = reader.IsDBNull("ContextTags") ? "[]" : reader.GetString("ContextTags")
                });
            }
            return files;
        }

        public async Task<int> CreateFileUploadAsync(FileUpload file)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO FileUploads (UserId, FileName, FilePath, FileType, FileSize, UploadedAt, IsProcessed, ProcessingStatus, Content, ProcessedContent, TokenCount, Subject, Topic, ContextTags) " +
                "VALUES (@userId, @fileName, @filePath, @fileType, @fileSize, @uploadedAt, @isProcessed, @processingStatus, @content, @processedContent, @tokenCount, @subject, @topic, @contextTags); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@userId", file.UserId);
            command.Parameters.AddWithValue("@fileName", file.FileName);
            command.Parameters.AddWithValue("@filePath", file.FilePath);
            command.Parameters.AddWithValue("@fileType", file.FileType);
            command.Parameters.AddWithValue("@fileSize", file.FileSize);
            command.Parameters.AddWithValue("@uploadedAt", file.UploadedAt);
            command.Parameters.AddWithValue("@isProcessed", file.IsProcessed);
            command.Parameters.AddWithValue("@processingStatus", file.ProcessingStatus);
            command.Parameters.AddWithValue("@content", file.Content);
            command.Parameters.AddWithValue("@processedContent", file.ProcessedContent);
            command.Parameters.AddWithValue("@tokenCount", file.TokenCount);
            command.Parameters.AddWithValue("@subject", file.Subject);
            command.Parameters.AddWithValue("@topic", file.Topic);
            command.Parameters.AddWithValue("@contextTags", file.ContextTags);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Analysis operations
        public async Task<int> CreateAnalysisResultAsync(AnalysisResult analysis)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO AnalysisResults (UserId, OverallScore, Feedback, StudyPlan, CreatedAt) " +
                "VALUES (@userId, @overallScore, @feedback, @studyPlan, @createdAt); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@userId", analysis.UserId);
            command.Parameters.AddWithValue("@overallScore", analysis.OverallScore);
            command.Parameters.AddWithValue("@feedback", analysis.Feedback);
            command.Parameters.AddWithValue("@studyPlan", analysis.StudyPlan);
            command.Parameters.AddWithValue("@createdAt", analysis.CreatedAt);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Study Session operations
        public async Task<int> CreateStudySessionAsync(StudySession session)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO StudySessions (UserId, SessionName, StartTime, EndTime, Notes, IsActive) " +
                "VALUES (@userId, @sessionName, @startTime, @endTime, @notes, @isActive); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@userId", session.UserId);
            command.Parameters.AddWithValue("@sessionName", session.SessionName);
            command.Parameters.AddWithValue("@startTime", session.StartTime);
            command.Parameters.AddWithValue("@endTime", session.EndTime);
            command.Parameters.AddWithValue("@notes", session.Notes);
            command.Parameters.AddWithValue("@isActive", session.IsActive);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<StudySession>> GetStudySessionsByUserIdAsync(int userId)
        {
            var sessions = new List<StudySession>();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM StudySessions WHERE UserId = @userId ORDER BY StartTime DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sessions.Add(new StudySession
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    SessionName = reader.GetString("SessionName"),
                    StartTime = reader.GetDateTime("StartTime"),
                    EndTime = reader.IsDBNull("EndTime") ? null : reader.GetDateTime("EndTime"),
                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }
            return sessions;
        }

        // Database initialization
        public async Task InitializeDatabaseAsync()
        {
            // First, try to create the database if it doesn't exist
            var createDbConnectionString = _connectionString.Replace("Database=StudentStudyAI;", "Database=mysql;");
            using var createDbConnection = new MySqlConnection(createDbConnectionString);
            await createDbConnection.OpenAsync();
            
            var createDbCommand = new MySqlCommand("CREATE DATABASE IF NOT EXISTS StudentStudyAI CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci", createDbConnection);
            await createDbCommand.ExecuteNonQueryAsync();
            
            // Now connect to the actual database
            using var connection = await GetConnectionAsync();
            
            // Drop and recreate tables to ensure correct schema
            var dropAndCreateTablesScript = @"
                -- Disable foreign key checks temporarily
                SET FOREIGN_KEY_CHECKS = 0;
                
                -- Drop tables if they exist (in reverse dependency order)
                DROP TABLE IF EXISTS ContentDifficultyAnalysis;
                DROP TABLE IF EXISTS QuizPerformance;
                DROP TABLE IF EXISTS KnowledgeProgression;
                DROP TABLE IF EXISTS UserLearningPreferences;
                DROP TABLE IF EXISTS UserKnowledgeProfiles;
                DROP TABLE IF EXISTS SubjectGroups;
                DROP TABLE IF EXISTS FileChunks;
                DROP TABLE IF EXISTS ChunkedFiles;
                DROP TABLE IF EXISTS AnalysisResults;
                DROP TABLE IF EXISTS QuizAttempts;
                DROP TABLE IF EXISTS StudySessions;
                DROP TABLE IF EXISTS StudyGuides;
                DROP TABLE IF EXISTS Quizzes;
                DROP TABLE IF EXISTS Conversations;
                DROP TABLE IF EXISTS FileUploads;
                DROP TABLE IF EXISTS Users;
                
                -- Re-enable foreign key checks
                SET FOREIGN_KEY_CHECKS = 1;

                -- Create tables with correct schema
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(50) NOT NULL UNIQUE,
                    Email VARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash VARCHAR(255) NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    LastLoginAt DATETIME NOT NULL,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                    IsAdmin BOOLEAN NOT NULL DEFAULT FALSE
                );

                CREATE TABLE IF NOT EXISTS FileUploads (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    FileName VARCHAR(255) NOT NULL,
                    FilePath VARCHAR(500) NOT NULL,
                    FileType VARCHAR(100) NOT NULL,
                    FileSize BIGINT NOT NULL,
                    UploadedAt DATETIME NOT NULL,
                    IsProcessed BOOLEAN NOT NULL DEFAULT FALSE,
                    ProcessingStatus VARCHAR(50),
                    Status VARCHAR(50),
                    Content LONGTEXT,
                    ProcessedContent LONGTEXT,
                    TokenCount INT NOT NULL DEFAULT 0,
                    Subject VARCHAR(255),
                    Topic VARCHAR(255),
                    StudentLevel VARCHAR(50),
                    ContextTags TEXT,
                    ExtractedContent LONGTEXT,
                    ProcessedAt DATETIME,
                    -- Phase 1: File Grouping fields
                    AutoDetectedSubject VARCHAR(255),
                    AutoDetectedTopic VARCHAR(255),
                    UserDefinedSubject VARCHAR(255),
                    UserDefinedTopic VARCHAR(255),
                    IsUserModified BOOLEAN DEFAULT FALSE,
                    SubjectGroupId INT,
                    -- Soft delete fields
                    IsDeleted BOOLEAN DEFAULT FALSE,
                    DeletedAt DATETIME,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS AnalysisResults (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    OverallScore DECIMAL(5,2) NOT NULL,
                    Feedback TEXT NOT NULL,
                    StudyPlan TEXT NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS StudySessions (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    SessionName VARCHAR(100) NOT NULL,
                    StartTime DATETIME NOT NULL,
                    EndTime DATETIME,
                    Notes TEXT,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS StudyGuides (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    Title VARCHAR(255) NOT NULL DEFAULT 'Study Guide',
                    Content LONGTEXT NOT NULL,
                    Subject VARCHAR(255),
                    Topic VARCHAR(255),
                    SourceFileIds TEXT,
                    CreatedAt DATETIME NOT NULL,
                    UpdatedAt DATETIME,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );



                CREATE TABLE IF NOT EXISTS ChunkedFiles (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    FileUploadId INT NOT NULL,
                    ChunkIndex INT NOT NULL,
                    Content LONGTEXT NOT NULL,
                    TokenCount INT NOT NULL,
                    IsProcessed BOOLEAN NOT NULL DEFAULT FALSE,
                    CreatedAt DATETIME NOT NULL,
                    FOREIGN KEY (FileUploadId) REFERENCES FileUploads(Id) ON DELETE CASCADE
                );
            ";

            var command = new MySqlCommand(dropAndCreateTablesScript, connection);
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateDatabaseSchemaAsync()
        {
            using var connection = await GetConnectionAsync();
            
            // Find the script file in various possible locations
            var possiblePaths = new[]
            {
                "Scripts/update_database_schema.sql",
                "../Scripts/update_database_schema.sql",
                "../../Scripts/update_database_schema.sql",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "update_database_schema.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Scripts", "update_database_schema.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Scripts", "update_database_schema.sql")
            };

            string? scriptPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    scriptPath = path;
                    break;
                }
            }

            if (scriptPath == null)
            {
                // If no script file found, create tables manually
                await CreateTablesManually(connection);
                return;
            }

            // Read and execute the update schema script
            var updateScript = await File.ReadAllTextAsync(scriptPath);
            var command = new MySqlCommand(updateScript, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task CreateTablesManually(MySqlConnection connection)
        {
            var createTablesScript = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(50) UNIQUE NOT NULL,
                    Email VARCHAR(100) UNIQUE NOT NULL,
                    PasswordHash VARCHAR(255) NOT NULL,
                    FirstName VARCHAR(50),
                    LastName VARCHAR(50),
                    IsActive BOOLEAN DEFAULT TRUE,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastLoginAt DATETIME
                );

                CREATE TABLE IF NOT EXISTS AnalysisResults (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    FileId INT NOT NULL,
                    AnalysisType VARCHAR(50) NOT NULL,
                    Content LONGTEXT NOT NULL,
                    Summary TEXT,
                    KeyPoints TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (FileId) REFERENCES FileUploads(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS StudySessions (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    SessionName VARCHAR(100) NOT NULL,
                    StartTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    EndTime DATETIME,
                    Duration INT,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Conversations (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    Prompt TEXT NOT NULL,
                    Response TEXT NOT NULL,
                    ContextFileIds TEXT,
                    ContextStudyGuideIds TEXT,
                    Subject VARCHAR(255),
                    Topic VARCHAR(255),
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Quizzes (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    Title VARCHAR(255) NOT NULL DEFAULT 'Quiz',
                    Subject VARCHAR(255),
                    Topic VARCHAR(255),
                    Level VARCHAR(50),
                    Status VARCHAR(50) NOT NULL DEFAULT 'completed',
                    Questions LONGTEXT NOT NULL,
                    SourceFileIds TEXT,
                    SourceStudyGuideIds TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS QuizAttempts (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    QuizId INT NOT NULL,
                    UserId INT NOT NULL,
                    Answers LONGTEXT NOT NULL,
                    Score DECIMAL(5,2),
                    CompletedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                -- QuizPerformance table (depends on Quizzes table)
                CREATE TABLE IF NOT EXISTS QuizPerformance (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    QuizId INT NOT NULL,
                    Score DECIMAL(5,2),
                    KnowledgeLevel INT,
                    Difficulty INT,
                    TimeSpent INT,
                    CompletedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS UserSessions (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    SessionToken VARCHAR(255) UNIQUE NOT NULL,
                    ExpiresAt DATETIME NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                -- Phase 1: File Grouping tables
                CREATE TABLE IF NOT EXISTS SubjectGroups (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    GroupName VARCHAR(255) NOT NULL,
                    Description TEXT,
                    Color VARCHAR(7) DEFAULT '#3498db',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    IsActive BOOLEAN DEFAULT TRUE,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    UNIQUE KEY unique_user_group (UserId, GroupName)
                );

                -- Phase 3: Knowledge Tracking tables
                CREATE TABLE IF NOT EXISTS UserKnowledgeProfiles (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    Subject VARCHAR(255) NOT NULL,
                    KnowledgeLevel INT NOT NULL,
                    ConfidenceScore DECIMAL(3,2) DEFAULT 0.50,
                    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsActive BOOLEAN DEFAULT TRUE,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    UNIQUE KEY unique_user_subject (UserId, Subject)
                );

                CREATE TABLE IF NOT EXISTS KnowledgeProgression (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    Subject VARCHAR(255) NOT NULL,
                    PreviousLevel INT,
                    NewLevel INT,
                    ChangeReason VARCHAR(255),
                    ConfidenceScore DECIMAL(3,2),
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS UserLearningPreferences (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    PreferredQuizLength VARCHAR(50) DEFAULT 'standard',
                    CustomQuestionMultiplier DECIMAL(3,2) DEFAULT 1.0,
                    PreferredDifficulty VARCHAR(50) DEFAULT 'adaptive',
                    TimeAvailable INT DEFAULT 30,
                    StudyStyle VARCHAR(50) DEFAULT 'balanced',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    UNIQUE KEY unique_user_preferences (UserId)
                );

                CREATE TABLE IF NOT EXISTS ContentDifficultyAnalysis (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    FileId INT NOT NULL,
                    ComplexityScore DECIMAL(3,2),
                    KnowledgeLevel INT,
                    UniqueConcepts INT,
                    ContentVolume INT,
                    EstimatedQuestions INT,
                    TimeEstimate INT,
                    AnalyzedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (FileId) REFERENCES FileUploads(Id) ON DELETE CASCADE
                );

                -- Create indexes for better performance
                CREATE INDEX IF NOT EXISTS idx_fileuploads_subject_group ON FileUploads(SubjectGroupId);
                CREATE INDEX IF NOT EXISTS idx_fileuploads_auto_subject ON FileUploads(AutoDetectedSubject);
                CREATE INDEX IF NOT EXISTS idx_fileuploads_user_subject ON FileUploads(UserDefinedSubject);
                CREATE INDEX IF NOT EXISTS idx_subjectgroups_user ON SubjectGroups(UserId);
                CREATE INDEX IF NOT EXISTS idx_user_knowledge_profiles_user ON UserKnowledgeProfiles(UserId);
                CREATE INDEX IF NOT EXISTS idx_user_knowledge_profiles_subject ON UserKnowledgeProfiles(Subject);
                CREATE INDEX IF NOT EXISTS idx_quiz_performance_user ON QuizPerformance(UserId);
                CREATE INDEX IF NOT EXISTS idx_quiz_performance_quiz ON QuizPerformance(QuizId);
                CREATE INDEX IF NOT EXISTS idx_quiz_performance_completed ON QuizPerformance(CompletedAt);
                CREATE INDEX IF NOT EXISTS idx_knowledge_progression_user ON KnowledgeProgression(UserId);
                CREATE INDEX IF NOT EXISTS idx_knowledge_progression_subject ON KnowledgeProgression(Subject);
                CREATE INDEX IF NOT EXISTS idx_content_difficulty_file ON ContentDifficultyAnalysis(FileId);

                -- Insert default learning preferences for existing users
                INSERT IGNORE INTO UserLearningPreferences (UserId, PreferredQuizLength, PreferredDifficulty, TimeAvailable, StudyStyle)
                SELECT Id, 'standard', 'adaptive', 30, 'balanced'
                FROM Users
                WHERE Id NOT IN (SELECT UserId FROM UserLearningPreferences);

                -- Create guest user for non-authenticated access
                DELETE FROM Users WHERE Id = 0;
                INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt, LastLoginAt, IsActive)
                VALUES (0, 'guest', 'guest@system.local', '', NOW(), NOW(), TRUE);
            ";

            var command = new MySqlCommand(createTablesScript, connection);
            await command.ExecuteNonQueryAsync();
        }

        // Study Guide operations
        public async Task<int> CreateStudyGuideAsync(StudyGuide studyGuide)
        {
            try
            {
                // Log the incoming study guide for debugging
                _logger.LogInformation("Creating study guide - Title: '{Title}', UserId: {UserId}, Content length: {ContentLength}", 
                    studyGuide.Title ?? "NULL", studyGuide.UserId, studyGuide.Content?.Length ?? 0);

                // Validate required fields
                if (studyGuide.Title == null || string.IsNullOrWhiteSpace(studyGuide.Title))
                {
                    _logger.LogWarning("Study guide title was null or empty, setting default");
                    studyGuide.Title = "Study Guide";
                }
                
                // Ensure other required fields are set
                if (string.IsNullOrWhiteSpace(studyGuide.Content))
                {
                    _logger.LogWarning("Study guide content was null or empty, setting default");
                    studyGuide.Content = "No content available";
                }
                
                if (studyGuide.CreatedAt == default)
                {
                    studyGuide.CreatedAt = DateTime.UtcNow;
                }
                
                if (studyGuide.UpdatedAt == default)
                {
                    studyGuide.UpdatedAt = DateTime.UtcNow;
                }
                
                // Final safety check - ensure Title is never null
                if (studyGuide.Title == null)
                {
                    _logger.LogError("Title is still null after all checks - this should not happen!");
                    studyGuide.Title = "Study Guide";
                }

                // Log final values before database insert
                _logger.LogInformation("Final study guide values - Title: '{Title}', UserId: {UserId}, CreatedAt: {CreatedAt}", 
                    studyGuide.Title, studyGuide.UserId, studyGuide.CreatedAt);
            
                using var connection = await GetConnectionAsync();
                var command = new MySqlCommand(
                    "INSERT INTO StudyGuides (UserId, Title, Content, Subject, Topic, SourceFileIds, CreatedAt, UpdatedAt, IsActive) " +
                    "VALUES (@userId, @title, @content, @subject, @topic, @sourceFileIds, @createdAt, @updatedAt, @isActive); " +
                    "SELECT LAST_INSERT_ID();", connection);
                
                command.Parameters.AddWithValue("@userId", studyGuide.UserId);
                command.Parameters.AddWithValue("@title", studyGuide.Title);
                command.Parameters.AddWithValue("@content", studyGuide.Content);
                command.Parameters.AddWithValue("@subject", studyGuide.Subject);
                command.Parameters.AddWithValue("@topic", studyGuide.Topic);
                command.Parameters.AddWithValue("@sourceFileIds", studyGuide.SourceFileIds);
                command.Parameters.AddWithValue("@createdAt", studyGuide.CreatedAt);
                command.Parameters.AddWithValue("@updatedAt", studyGuide.UpdatedAt);
                command.Parameters.AddWithValue("@isActive", studyGuide.IsActive);

                // Log the actual SQL command and parameters for debugging
                _logger.LogInformation("Executing SQL with parameters - Title: '{Title}', UserId: {UserId}", 
                    studyGuide.Title, studyGuide.UserId);

                var result = await command.ExecuteScalarAsync();
                var studyGuideId = Convert.ToInt32(result);
                
                _logger.LogInformation("Study guide created successfully with ID: {StudyGuideId}", studyGuideId);
                return studyGuideId;
            }
            catch (MySqlException sqlEx)
            {
                _logger.LogError(sqlEx, "MySQL error creating study guide. Error Code: {ErrorCode}, Title: '{Title}', UserId: {UserId}", 
                    sqlEx.Number, studyGuide.Title ?? "NULL", studyGuide.UserId);
                
                // Provide more specific error messages based on MySQL error codes
                var errorMessage = sqlEx.Number switch
                {
                    1048 => $"Database constraint violation: Field 'Title' doesn't have a default value. Title was: '{studyGuide.Title ?? "NULL"}'",
                    1062 => "Duplicate entry error",
                    1452 => "Foreign key constraint violation",
                    _ => $"Database error (Code {sqlEx.Number}): {sqlEx.Message}"
                };
                
                throw new InvalidOperationException(errorMessage, sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating study guide. Title: '{Title}', UserId: {UserId}", 
                    studyGuide.Title ?? "NULL", studyGuide.UserId);
                throw;
            }
        }

        public async Task<List<StudyGuide>> GetStudyGuidesByUserIdAsync(int userId)
        {
            var studyGuides = new List<StudyGuide>();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM StudyGuides WHERE UserId = @userId AND IsActive = TRUE ORDER BY CreatedAt DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                studyGuides.Add(new StudyGuide
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    Title = reader.GetString("Title"),
                    Content = reader.GetString("Content"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    SourceFileIds = reader.IsDBNull("SourceFileIds") ? "[]" : reader.GetString("SourceFileIds"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }
            return studyGuides;
        }

        // Conversation operations
        public async Task<int> CreateConversationAsync(Conversation conversation)
        {
            using var connection = await GetConnectionAsync();
            
            // Ensure Conversations table exists
            await EnsureConversationsTableExistsAsync(connection);
            
            var command = new MySqlCommand(
                "INSERT INTO Conversations (UserId, Prompt, Response, ContextFileIds, ContextStudyGuideIds, Subject, Topic, CreatedAt) " +
                "VALUES (@userId, @prompt, @response, @contextFileIds, @contextStudyGuideIds, @subject, @topic, @createdAt); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@userId", conversation.UserId);
            command.Parameters.AddWithValue("@prompt", conversation.Prompt);
            command.Parameters.AddWithValue("@response", conversation.Response);
            command.Parameters.AddWithValue("@contextFileIds", conversation.ContextFileIds);
            command.Parameters.AddWithValue("@contextStudyGuideIds", conversation.ContextStudyGuideIds);
            command.Parameters.AddWithValue("@subject", conversation.Subject);
            command.Parameters.AddWithValue("@topic", conversation.Topic);
            command.Parameters.AddWithValue("@createdAt", conversation.CreatedAt);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private async Task EnsureConversationsTableExistsAsync(MySqlConnection connection)
        {
            try
            {
                var checkTableCommand = new MySqlCommand("SHOW TABLES LIKE 'Conversations'", connection);
                var tableExists = await checkTableCommand.ExecuteScalarAsync();
                
                if (tableExists == null)
                {
                    _logger.LogWarning("Conversations table does not exist, creating it...");
                    
                    var createTableCommand = new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS Conversations (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            UserId INT NOT NULL,
                            Prompt TEXT NOT NULL,
                            Response TEXT NOT NULL,
                            ContextFileIds TEXT,
                            ContextStudyGuideIds TEXT,
                            Subject VARCHAR(255),
                            Topic VARCHAR(255),
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        )", connection);
                    
                    await createTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Conversations table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring Conversations table exists");
                throw;
            }
        }

        private async Task EnsureQuizzesTableExistsAsync(MySqlConnection connection)
        {
            try
            {
                var checkTableCommand = new MySqlCommand("SHOW TABLES LIKE 'Quizzes'", connection);
                var tableExists = await checkTableCommand.ExecuteScalarAsync();
                
                if (tableExists == null)
                {
                    _logger.LogWarning("Quizzes table does not exist, creating it...");
                    
                    var createTableCommand = new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS Quizzes (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            UserId INT NOT NULL,
                            Title VARCHAR(255) NOT NULL DEFAULT 'Quiz',
                            Subject VARCHAR(255),
                            Topic VARCHAR(255),
                            Level VARCHAR(50),
                            Status VARCHAR(50) NOT NULL DEFAULT 'completed',
                            Questions LONGTEXT NOT NULL,
                            SourceFileIds TEXT,
                            SourceStudyGuideIds TEXT,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                            IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        )", connection);
                    
                    await createTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Quizzes table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring Quizzes table exists");
                throw;
            }
        }

        private async Task EnsureQuizPerformanceTableExistsAsync()
        {
            using var connection = await GetConnectionAsync();
            try
            {
                var checkTableCommand = new MySqlCommand("SHOW TABLES LIKE 'QuizPerformance'", connection);
                var tableExists = await checkTableCommand.ExecuteScalarAsync();
                
                if (tableExists == null)
                {
                    _logger.LogWarning("QuizPerformance table does not exist, creating it...");
                    
                    var createTableCommand = new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS QuizPerformance (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            UserId INT NOT NULL,
                            QuizId INT NOT NULL,
                            Score DECIMAL(5,2),
                            KnowledgeLevel VARCHAR(50),
                            Difficulty VARCHAR(20),
                            TimeSpent INT,
                            CompletedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                            FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE
                        )", connection);
                    
                    await createTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("QuizPerformance table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring QuizPerformance table exists");
                throw;
            }
        }

        private async Task EnsureUserKnowledgeProfilesTableExistsAsync()
        {
            using var connection = await GetConnectionAsync();
            try
            {
                var checkTableCommand = new MySqlCommand("SHOW TABLES LIKE 'UserKnowledgeProfiles'", connection);
                var tableExists = await checkTableCommand.ExecuteScalarAsync();
                
                if (tableExists == null)
                {
                    _logger.LogWarning("UserKnowledgeProfiles table does not exist, creating it...");
                    
                    var createTableCommand = new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS UserKnowledgeProfiles (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            UserId INT NOT NULL,
                            Subject VARCHAR(100) NOT NULL,
                            KnowledgeLevel VARCHAR(50),
                            ConfidenceScore DECIMAL(5,2),
                            LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            IsActive BOOLEAN DEFAULT TRUE,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        )", connection);
                    
                    await createTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("UserKnowledgeProfiles table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring UserKnowledgeProfiles table exists");
                throw;
            }
        }

        private async Task EnsureKnowledgeProgressionTableExistsAsync()
        {
            using var connection = await GetConnectionAsync();
            try
            {
                var checkTableCommand = new MySqlCommand("SHOW TABLES LIKE 'KnowledgeProgression'", connection);
                var tableExists = await checkTableCommand.ExecuteScalarAsync();
                
                if (tableExists == null)
                {
                    _logger.LogWarning("KnowledgeProgression table does not exist, creating it...");
                    
                    var createTableCommand = new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS KnowledgeProgression (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            UserId INT NOT NULL,
                            Subject VARCHAR(100) NOT NULL,
                            PreviousLevel VARCHAR(50),
                            NewLevel VARCHAR(50),
                            ChangeReason VARCHAR(255),
                            ChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        )", connection);
                    
                    await createTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("KnowledgeProgression table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring KnowledgeProgression table exists");
                throw;
            }
        }

        private async Task EnsureUserLearningPreferencesTableExistsAsync()
        {
            using var connection = await GetConnectionAsync();
            try
            {
                var checkTableCommand = new MySqlCommand("SHOW TABLES LIKE 'UserLearningPreferences'", connection);
                var tableExists = await checkTableCommand.ExecuteScalarAsync();
                
                if (tableExists == null)
                {
                    _logger.LogWarning("UserLearningPreferences table does not exist, creating it...");
                    
                    var createTableCommand = new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS UserLearningPreferences (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            UserId INT NOT NULL,
                            PreferredQuizLength VARCHAR(50) DEFAULT 'standard',
                            CustomQuestionMultiplier DECIMAL(3,2) DEFAULT 1.0,
                            PreferredDifficulty VARCHAR(50) DEFAULT 'adaptive',
                            TimeAvailable INT DEFAULT 30,
                            StudyStyle VARCHAR(50) DEFAULT 'balanced',
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        )", connection);
                    
                    await createTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("UserLearningPreferences table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring UserLearningPreferences table exists");
                throw;
            }
        }

        private async Task EnsureContentDifficultyAnalysisTableExistsAsync()
        {
            using var connection = await GetConnectionAsync();
            try
            {
                var checkTableCommand = new MySqlCommand("SHOW TABLES LIKE 'ContentDifficultyAnalysis'", connection);
                var tableExists = await checkTableCommand.ExecuteScalarAsync();
                
                if (tableExists == null)
                {
                    _logger.LogWarning("ContentDifficultyAnalysis table does not exist, creating it...");
                    
                    var createTableCommand = new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS ContentDifficultyAnalysis (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            FileId INT NOT NULL,
                            ComplexityScore DECIMAL(3,2),
                            KnowledgeLevel INT,
                            UniqueConcepts INT,
                            ContentVolume INT,
                            EstimatedQuestions INT,
                            TimeEstimate INT,
                            AnalyzedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (FileId) REFERENCES FileUploads(Id) ON DELETE CASCADE
                        )", connection);
                    
                    await createTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("ContentDifficultyAnalysis table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring ContentDifficultyAnalysis table exists");
                throw;
            }
        }

        private async Task EnsureQuizAttemptsTableExistsAsync(MySqlConnection connection)
        {
            try
            {
                var checkTableCommand = new MySqlCommand("SHOW TABLES LIKE 'QuizAttempts'", connection);
                var tableExists = await checkTableCommand.ExecuteScalarAsync();
                
                if (tableExists == null)
                {
                    _logger.LogWarning("QuizAttempts table does not exist, creating it...");
                    
                    var createTableCommand = new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS QuizAttempts (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            QuizId INT NOT NULL,
                            UserId INT NOT NULL,
                            Answers LONGTEXT NOT NULL,
                            Score DECIMAL(5,2),
                            CompletedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        )", connection);
                    
                    await createTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("QuizAttempts table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring QuizAttempts table exists");
                throw;
            }
        }

        public async Task<List<Conversation>> GetConversationsByUserIdAsync(int userId, int limit = 10)
        {
            var conversations = new List<Conversation>();
            using var connection = await GetConnectionAsync();
            
            // Check if Conversations table exists, if not create it
            await EnsureConversationsTableExistsAsync(connection);
            
            var command = new MySqlCommand("SELECT * FROM Conversations WHERE UserId = @userId ORDER BY CreatedAt DESC LIMIT @limit", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@limit", limit);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conversations.Add(new Conversation
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    Prompt = reader.GetString("Prompt"),
                    Response = reader.GetString("Response"),
                    ContextFileIds = reader.IsDBNull("ContextFileIds") ? "[]" : reader.GetString("ContextFileIds"),
                    ContextStudyGuideIds = reader.IsDBNull("ContextStudyGuideIds") ? "[]" : reader.GetString("ContextStudyGuideIds"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }
            return conversations;
        }

        // User Session operations
        public async Task<int> CreateUserSessionAsync(UserSession session)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO UserSessions (UserId, SessionToken, ContextWindow, LastActivity, ExpiresAt, IsActive) " +
                "VALUES (@userId, @sessionToken, @contextWindow, @lastActivity, @expiresAt, @isActive); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@userId", session.UserId);
            command.Parameters.AddWithValue("@sessionToken", session.SessionToken);
            command.Parameters.AddWithValue("@contextWindow", session.ContextWindow);
            command.Parameters.AddWithValue("@lastActivity", session.LastActivity);
            command.Parameters.AddWithValue("@expiresAt", session.ExpiresAt);
            command.Parameters.AddWithValue("@isActive", session.IsActive);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<UserSession?> GetUserSessionByTokenAsync(string sessionToken)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM UserSessions WHERE SessionToken = @sessionToken AND IsActive = TRUE AND ExpiresAt > NOW()", connection);
            command.Parameters.AddWithValue("@sessionToken", sessionToken);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserSession
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    SessionToken = reader.GetString("SessionToken"),
                    ContextWindow = reader.IsDBNull("ContextWindow") ? "{}" : reader.GetString("ContextWindow"),
                    LastActivity = reader.GetDateTime("LastActivity"),
                    ExpiresAt = reader.GetDateTime("ExpiresAt"),
                    IsActive = reader.GetBoolean("IsActive")
                };
            }
            return null;
        }

        public async Task UpdateUserSessionAsync(UserSession session)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE UserSessions SET ContextWindow = @contextWindow, LastActivity = @lastActivity, ExpiresAt = @expiresAt WHERE Id = @id", connection);
            
            command.Parameters.AddWithValue("@id", session.Id);
            command.Parameters.AddWithValue("@contextWindow", session.ContextWindow);
            command.Parameters.AddWithValue("@lastActivity", session.LastActivity);
            command.Parameters.AddWithValue("@expiresAt", session.ExpiresAt);

            await command.ExecuteNonQueryAsync();
        }

        // Context retrieval operations
        public async Task<List<FileUpload>> GetRelevantFilesForContextAsync(int userId, string? subject = null, string? topic = null, int limit = 5)
        {
            var files = new List<FileUpload>();
            using var connection = await GetConnectionAsync();
            
            string sql = "SELECT * FROM FileUploads WHERE UserId = @userId AND IsProcessed = TRUE";
            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@limit", limit)
            };

            if (!string.IsNullOrEmpty(subject))
            {
                sql += " AND Subject = @subject";
                parameters.Add(new MySqlParameter("@subject", subject));
            }

            if (!string.IsNullOrEmpty(topic))
            {
                sql += " AND Topic LIKE @topic";
                parameters.Add(new MySqlParameter("@topic", $"%{topic}%"));
            }

            sql += " ORDER BY UploadedAt DESC LIMIT @limit";

            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddRange(parameters.ToArray());

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                files.Add(new FileUpload
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    FileName = reader.GetString("FileName"),
                    FilePath = reader.GetString("FilePath"),
                    FileType = reader.GetString("FileType"),
                    FileSize = reader.GetInt64("FileSize"),
                    UploadedAt = reader.GetDateTime("UploadedAt"),
                    IsProcessed = reader.GetBoolean("IsProcessed"),
                    ProcessingStatus = reader.IsDBNull("ProcessingStatus") ? null : reader.GetString("ProcessingStatus"),
                    Content = reader.IsDBNull("Content") ? null : reader.GetString("Content"),
                    ProcessedContent = reader.IsDBNull("ProcessedContent") ? null : reader.GetString("ProcessedContent"),
                    TokenCount = reader.GetInt32("TokenCount"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    ContextTags = reader.IsDBNull("ContextTags") ? "[]" : reader.GetString("ContextTags")
                });
            }
            return files;
        }

        // Quiz operations
        public async Task<int> CreateQuizAsync(Quiz quiz)
        {
            using var connection = await GetConnectionAsync();
            
            // Ensure Quizzes table exists
            await EnsureQuizzesTableExistsAsync(connection);
            
            var command = new MySqlCommand(
                "INSERT INTO Quizzes (UserId, Title, Questions, SourceFileIds, SourceStudyGuideIds, Subject, Topic, CreatedAt, IsActive) " +
                "VALUES (@userId, @title, @questions, @sourceFileIds, @sourceStudyGuideIds, @subject, @topic, @createdAt, @isActive); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@userId", quiz.UserId);
            command.Parameters.AddWithValue("@title", quiz.Title);
            command.Parameters.AddWithValue("@questions", quiz.Questions);
            command.Parameters.AddWithValue("@sourceFileIds", quiz.SourceFileIds);
            command.Parameters.AddWithValue("@sourceStudyGuideIds", quiz.SourceStudyGuideIds);
            command.Parameters.AddWithValue("@subject", quiz.Subject);
            command.Parameters.AddWithValue("@topic", quiz.Topic);
            command.Parameters.AddWithValue("@createdAt", quiz.CreatedAt);
            command.Parameters.AddWithValue("@isActive", quiz.IsActive);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<Quiz>> GetQuizzesByUserIdAsync(int userId)
        {
            var quizzes = new List<Quiz>();
            using var connection = await GetConnectionAsync();
            
            // Ensure Quizzes table exists
            await EnsureQuizzesTableExistsAsync(connection);
            
            var command = new MySqlCommand("SELECT * FROM Quizzes WHERE UserId = @userId AND IsActive = TRUE ORDER BY CreatedAt DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                quizzes.Add(new Quiz
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    Title = reader.GetString("Title"),
                    Questions = reader.IsDBNull("Questions") ? "[]" : reader.GetString("Questions"),
                    SourceFileIds = reader.IsDBNull("SourceFileIds") ? "[]" : reader.GetString("SourceFileIds"),
                    SourceStudyGuideIds = reader.IsDBNull("SourceStudyGuideIds") ? "[]" : reader.GetString("SourceStudyGuideIds"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }
            return quizzes;
        }

        public async Task<int> CreateQuizAttemptAsync(QuizAttempt attempt)
        {
            using var connection = await GetConnectionAsync();
            
            // Ensure QuizAttempts table exists
            await EnsureQuizAttemptsTableExistsAsync(connection);
            
            var command = new MySqlCommand(
                "INSERT INTO QuizAttempts (QuizId, UserId, Answers, Score, CompletedAt) " +
                "VALUES (@quizId, @userId, @answers, @score, @completedAt); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@quizId", attempt.QuizId);
            command.Parameters.AddWithValue("@userId", attempt.UserId);
            command.Parameters.AddWithValue("@answers", attempt.Answers);
            command.Parameters.AddWithValue("@score", attempt.Score);
            command.Parameters.AddWithValue("@completedAt", attempt.CompletedAt);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // File Upload Methods
        public async Task<FileUpload?> GetFileUploadAsync(int fileId)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT * FROM FileUploads WHERE Id = @fileId", connection);
            command.Parameters.AddWithValue("@fileId", fileId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new FileUpload
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    FileName = reader.GetString("FileName"),
                    FilePath = reader.GetString("FilePath"),
                    FileType = reader.GetString("FileType"),
                    FileSize = reader.GetInt64("FileSize"),
                    UploadedAt = reader.GetDateTime("UploadedAt"),
                    IsProcessed = reader.GetBoolean("IsProcessed"),
                    ProcessingStatus = reader.IsDBNull("ProcessingStatus") ? null : reader.GetString("ProcessingStatus"),
                    StudentLevel = reader.IsDBNull("StudentLevel") ? null : reader.GetString("StudentLevel"),
                    Status = reader.IsDBNull("Status") ? "uploaded" : reader.GetString("Status"),
                    ProcessedAt = reader.IsDBNull("ProcessedAt") ? null : reader.GetDateTime("ProcessedAt"),
                    ExtractedContent = reader.IsDBNull("ExtractedContent") ? null : reader.GetString("ExtractedContent"),
                    Content = reader.IsDBNull("Content") ? null : reader.GetString("Content"),
                    ProcessedContent = reader.IsDBNull("ProcessedContent") ? null : reader.GetString("ProcessedContent"),
                    TokenCount = reader.GetInt32("TokenCount"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    ContextTags = reader.IsDBNull("ContextTags") ? "[]" : reader.GetString("ContextTags")
                };
            }
            return null;
        }

        public async Task<List<FileUpload>> GetFileUploadsByUserIdAsync(int userId)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT * FROM FileUploads WHERE UserId = @userId AND IsDeleted = FALSE ORDER BY UploadedAt DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            var files = new List<FileUpload>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                files.Add(new FileUpload
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    FileName = reader.GetString("FileName"),
                    FilePath = reader.GetString("FilePath"),
                    FileType = reader.GetString("FileType"),
                    FileSize = reader.GetInt64("FileSize"),
                    UploadedAt = reader.GetDateTime("UploadedAt"),
                    IsProcessed = reader.GetBoolean("IsProcessed"),
                    ProcessingStatus = reader.IsDBNull("ProcessingStatus") ? null : reader.GetString("ProcessingStatus"),
                    StudentLevel = reader.IsDBNull("StudentLevel") ? null : reader.GetString("StudentLevel"),
                    Status = reader.IsDBNull("Status") ? "uploaded" : reader.GetString("Status"),
                    ProcessedAt = reader.IsDBNull("ProcessedAt") ? null : reader.GetDateTime("ProcessedAt"),
                    ExtractedContent = reader.IsDBNull("ExtractedContent") ? null : reader.GetString("ExtractedContent"),
                    Content = reader.IsDBNull("Content") ? null : reader.GetString("Content"),
                    ProcessedContent = reader.IsDBNull("ProcessedContent") ? null : reader.GetString("ProcessedContent"),
                    TokenCount = reader.GetInt32("TokenCount"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    ContextTags = reader.IsDBNull("ContextTags") ? "[]" : reader.GetString("ContextTags"),
                    AutoDetectedSubject = reader.IsDBNull("AutoDetectedSubject") ? null : reader.GetString("AutoDetectedSubject"),
                    AutoDetectedTopic = reader.IsDBNull("AutoDetectedTopic") ? null : reader.GetString("AutoDetectedTopic"),
                    UserDefinedSubject = reader.IsDBNull("UserDefinedSubject") ? null : reader.GetString("UserDefinedSubject"),
                    UserDefinedTopic = reader.IsDBNull("UserDefinedTopic") ? null : reader.GetString("UserDefinedTopic"),
                    IsUserModified = reader.GetBoolean("IsUserModified"),
                    SubjectGroupId = reader.IsDBNull("SubjectGroupId") ? null : reader.GetInt32("SubjectGroupId"),
                    IsDeleted = reader.GetBoolean("IsDeleted"),
                    DeletedAt = reader.IsDBNull("DeletedAt") ? null : reader.GetDateTime("DeletedAt")
                });
            }
            return files;
        }

        public async Task<bool> DeleteFileUploadAsync(int fileId)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "DELETE FROM FileUploads WHERE Id = @fileId", connection);
            command.Parameters.AddWithValue("@fileId", fileId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateFileProcessingStatusAsync(int fileId, string status, string? extractedContent = null)
        {
            using var connection = await GetConnectionAsync();
            
            // If we have extracted content, try to detect subject and topic
            string? detectedSubject = null;
            string? detectedTopic = null;
            
            if (!string.IsNullOrEmpty(extractedContent))
            {
                // Use ContextService to detect subject and topic from content
                var contextService = new ContextService(this);
                detectedSubject = contextService.DetectSubjectFromContent(extractedContent);
                detectedTopic = contextService.DetectTopicFromContent(extractedContent);
            }
            
            var command = new MySqlCommand(
                "UPDATE FileUploads SET ProcessingStatus = @status, ProcessedAt = @processedAt, ExtractedContent = @extractedContent, Subject = @subject, Topic = @topic WHERE Id = @fileId", connection);
            command.Parameters.AddWithValue("@fileId", fileId);
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@processedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@extractedContent", extractedContent ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@subject", detectedSubject ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@topic", detectedTopic ?? (object)DBNull.Value);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // User Methods
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT * FROM Users WHERE Email = @email", connection);
            command.Parameters.AddWithValue("@email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32("Id"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    LastLoginAt = reader.GetDateTime("LastLoginAt"),
                    IsActive = reader.GetBoolean("IsActive"),
                    IsAdmin = reader.GetBoolean("IsAdmin")
                };
            }
            return null;
        }

        public async Task<bool> UpdateUserLastLoginAsync(int userId)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE Users SET LastLoginAt = @lastLoginAt WHERE Id = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@lastLoginAt", DateTime.UtcNow);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE Users SET Username = @username, Email = @email, PasswordHash = @passwordHash, IsActive = @isActive, IsAdmin = @isAdmin WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@isActive", user.IsActive);
            command.Parameters.AddWithValue("@isAdmin", user.IsAdmin);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE Users SET IsActive = false WHERE Id = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Analysis Result operations
        public async Task<AnalysisResult?> GetAnalysisResultByIdAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM AnalysisResults WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AnalysisResult
                {
                    Id = reader.GetInt32("Id"),
                    FileUploadId = reader.IsDBNull("FileUploadId") ? null : reader.GetInt32("FileUploadId"),
                    UserId = reader.IsDBNull("UserId") ? null : reader.GetInt32("UserId"),
                    OverallScore = reader.IsDBNull("OverallScore") ? 0 : reader.GetDecimal("OverallScore"),
                    Feedback = reader.IsDBNull("Feedback") ? null : reader.GetString("Feedback") ?? "",
                    StudyPlan = reader.IsDBNull("StudyPlan") ? null : reader.GetString("StudyPlan") ?? "",
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }
            return null;
        }

        public async Task<List<AnalysisResult>> GetAnalysisResultsByUserIdAsync(int userId)
        {
            var analyses = new List<AnalysisResult>();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM AnalysisResults WHERE UserId = @userId ORDER BY CreatedAt DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                analyses.Add(new AnalysisResult
                {
                    Id = reader.GetInt32("Id"),
                    FileUploadId = reader.IsDBNull("FileUploadId") ? null : reader.GetInt32("FileUploadId"),
                    UserId = reader.IsDBNull("UserId") ? null : reader.GetInt32("UserId"),
                    OverallScore = reader.IsDBNull("OverallScore") ? 0 : reader.GetDecimal("OverallScore"),
                    Feedback = reader.IsDBNull("Feedback") ? null : reader.GetString("Feedback") ?? "",
                    StudyPlan = reader.IsDBNull("StudyPlan") ? null : reader.GetString("StudyPlan") ?? "",
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }
            return analyses;
        }

        public async Task<bool> DeleteAnalysisResultAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("DELETE FROM AnalysisResults WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Study Session operations
        public async Task<StudySession?> GetStudySessionByIdAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("SELECT * FROM StudySessions WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new StudySession
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    SessionName = reader.GetString("SessionName"),
                    StartTime = reader.GetDateTime("StartTime"),
                    EndTime = reader.IsDBNull("EndTime") ? null : reader.GetDateTime("EndTime"),
                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                    IsActive = reader.GetBoolean("IsActive")
                };
            }
            return null;
        }

        public async Task<bool> UpdateStudySessionAsync(StudySession session)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(@"
                UPDATE StudySessions 
                SET SessionName = @sessionName, 
                    EndTime = @endTime, 
                    Notes = @notes, 
                    FileIdsJson = @fileIdsJson, 
                    IsActive = @isActive 
                WHERE Id = @id", connection);
            
            command.Parameters.AddWithValue("@id", session.Id);
            command.Parameters.AddWithValue("@sessionName", session.SessionName);
            command.Parameters.AddWithValue("@endTime", session.EndTime.HasValue ? session.EndTime.Value : (object)DBNull.Value);
            command.Parameters.AddWithValue("@notes", string.IsNullOrEmpty(session.Notes) ? (object)DBNull.Value : session.Notes);
            command.Parameters.AddWithValue("@fileIdsJson", session.FileIdsJson);
            command.Parameters.AddWithValue("@isActive", session.IsActive);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteStudySessionAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("DELETE FROM StudySessions WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // User operations
        public async Task<bool> DeleteUserAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand("DELETE FROM Users WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Subject Group operations
        public async Task<int> CreateSubjectGroupAsync(SubjectGroup subjectGroup)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO SubjectGroups (UserId, GroupName, Description, Color, CreatedAt, UpdatedAt, IsActive) " +
                "VALUES (@userId, @groupName, @description, @color, @createdAt, @updatedAt, @isActive); " +
                "SELECT LAST_INSERT_ID();", connection);

            command.Parameters.AddWithValue("@userId", subjectGroup.UserId);
            command.Parameters.AddWithValue("@groupName", subjectGroup.GroupName);
            command.Parameters.AddWithValue("@description", subjectGroup.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@color", subjectGroup.Color);
            command.Parameters.AddWithValue("@createdAt", subjectGroup.CreatedAt);
            command.Parameters.AddWithValue("@updatedAt", subjectGroup.UpdatedAt);
            command.Parameters.AddWithValue("@isActive", subjectGroup.IsActive);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<SubjectGroup?> GetSubjectGroupAsync(int groupId)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT Id, UserId, GroupName, Description, Color, CreatedAt, UpdatedAt, IsActive " +
                "FROM SubjectGroups WHERE Id = @groupId AND IsActive = TRUE", connection);
            command.Parameters.AddWithValue("@groupId", groupId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new SubjectGroup
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    GroupName = reader.GetString("GroupName"),
                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                    Color = reader.GetString("Color"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt"),
                    IsActive = reader.GetBoolean("IsActive")
                };
            }
            return null;
        }

        public async Task<List<SubjectGroup>> GetSubjectGroupsByUserIdAsync(int userId)
        {
            await EnsureSubjectGroupsTableExistsAsync();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT Id, UserId, GroupName, Description, Color, CreatedAt, UpdatedAt, IsActive " +
                "FROM SubjectGroups WHERE UserId = @userId AND IsActive = TRUE " +
                "ORDER BY GroupName", connection);
            command.Parameters.AddWithValue("@userId", userId);

            var groups = new List<SubjectGroup>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                groups.Add(new SubjectGroup
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    GroupName = reader.GetString("GroupName"),
                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                    Color = reader.GetString("Color"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }
            return groups;
        }

        private async Task EnsureSubjectGroupsTableExistsAsync()
        {
            using var connection = await GetConnectionAsync();
            var sql = @"CREATE TABLE IF NOT EXISTS SubjectGroups (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                UserId INT NOT NULL,
                GroupName VARCHAR(255) NOT NULL,
                Description VARCHAR(500),
                Color VARCHAR(20) NOT NULL DEFAULT '#888888',
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
            );";
            var command = new MySqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> UpdateSubjectGroupAsync(SubjectGroup subjectGroup)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE SubjectGroups SET GroupName = @groupName, Description = @description, " +
                "Color = @color, UpdatedAt = @updatedAt WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", subjectGroup.Id);
            command.Parameters.AddWithValue("@groupName", subjectGroup.GroupName);
            command.Parameters.AddWithValue("@description", subjectGroup.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@color", subjectGroup.Color);
            command.Parameters.AddWithValue("@updatedAt", subjectGroup.UpdatedAt);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteSubjectGroupAsync(int groupId)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE SubjectGroups SET IsActive = FALSE WHERE Id = @groupId", connection);
            command.Parameters.AddWithValue("@groupId", groupId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateFileUploadAsync(FileUpload file)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE FileUploads SET UserDefinedSubject = @userSubject, UserDefinedTopic = @userTopic, " +
                "SubjectGroupId = @subjectGroupId, IsUserModified = @isUserModified " +
                "WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", file.Id);
            command.Parameters.AddWithValue("@userSubject", file.UserDefinedSubject ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@userTopic", file.UserDefinedTopic ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@subjectGroupId", file.SubjectGroupId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@isUserModified", file.IsUserModified);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateFileAutoDetectionAsync(int fileId, string? autoDetectedSubject, string? autoDetectedTopic)
        {
            _logger.LogInformation("🔄 Updating file {FileId} with auto-detected subject: '{Subject}', topic: '{Topic}'", 
                fileId, autoDetectedSubject, autoDetectedTopic);
                
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE FileUploads SET AutoDetectedSubject = @autoSubject, AutoDetectedTopic = @autoTopic " +
                "WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", fileId);
            command.Parameters.AddWithValue("@autoSubject", autoDetectedSubject ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@autoTopic", autoDetectedTopic ?? (object)DBNull.Value);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            _logger.LogInformation("💾 Database update completed. Rows affected: {RowsAffected}", rowsAffected);
            return rowsAffected > 0;
        }

        // Knowledge Tracking operations
        public async Task<int> CreateUserKnowledgeProfileAsync(UserKnowledgeProfile profile)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO UserKnowledgeProfiles (UserId, Subject, KnowledgeLevel, ConfidenceScore, LastUpdated, CreatedAt, IsActive) " +
                "VALUES (@userId, @subject, @knowledgeLevel, @confidenceScore, @lastUpdated, @createdAt, @isActive); " +
                "SELECT LAST_INSERT_ID();", connection);

            command.Parameters.AddWithValue("@userId", profile.UserId);
            command.Parameters.AddWithValue("@subject", profile.Subject);
            command.Parameters.AddWithValue("@knowledgeLevel", profile.KnowledgeLevel);
            command.Parameters.AddWithValue("@confidenceScore", profile.ConfidenceScore);
            command.Parameters.AddWithValue("@lastUpdated", profile.LastUpdated);
            command.Parameters.AddWithValue("@createdAt", profile.CreatedAt);
            command.Parameters.AddWithValue("@isActive", profile.IsActive);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<UserKnowledgeProfile?> GetUserKnowledgeProfileAsync(int userId, string subject)
        {
            await EnsureUserKnowledgeProfilesTableExistsAsync();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT Id, UserId, Subject, KnowledgeLevel, ConfidenceScore, LastUpdated, CreatedAt, IsActive " +
                "FROM UserKnowledgeProfiles WHERE UserId = @userId AND Subject = @subject AND IsActive = TRUE", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@subject", subject);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserKnowledgeProfile
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    Subject = reader.GetString("Subject"),
                    KnowledgeLevel = reader.GetInt32("KnowledgeLevel"),
                    ConfidenceScore = reader.GetDecimal("ConfidenceScore"),
                    LastUpdated = reader.GetDateTime("LastUpdated"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    IsActive = reader.GetBoolean("IsActive")
                };
            }
            return null;
        }

        public async Task<List<UserKnowledgeProfile>> GetUserKnowledgeProfilesAsync(int userId)
        {
            await EnsureUserKnowledgeProfilesTableExistsAsync();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT Id, UserId, Subject, KnowledgeLevel, ConfidenceScore, LastUpdated, CreatedAt, IsActive " +
                "FROM UserKnowledgeProfiles WHERE UserId = @userId AND IsActive = TRUE " +
                "ORDER BY Subject", connection);
            command.Parameters.AddWithValue("@userId", userId);

            var profiles = new List<UserKnowledgeProfile>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                profiles.Add(new UserKnowledgeProfile
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    Subject = reader.GetString("Subject"),
                    KnowledgeLevel = reader.GetInt32("KnowledgeLevel"),
                    ConfidenceScore = reader.GetDecimal("ConfidenceScore"),
                    LastUpdated = reader.GetDateTime("LastUpdated"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }
            return profiles;
        }


        public async Task<bool> UpdateUserKnowledgeProfileAsync(UserKnowledgeProfile profile)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE UserKnowledgeProfiles SET KnowledgeLevel = @knowledgeLevel, ConfidenceScore = @confidenceScore, " +
                "LastUpdated = @lastUpdated WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", profile.Id);
            command.Parameters.AddWithValue("@knowledgeLevel", profile.KnowledgeLevel);
            command.Parameters.AddWithValue("@confidenceScore", profile.ConfidenceScore);
            command.Parameters.AddWithValue("@lastUpdated", profile.LastUpdated);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<int> CreateQuizPerformanceAsync(QuizPerformance performance)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO QuizPerformance (UserId, QuizId, Score, KnowledgeLevel, Difficulty, TimeSpent, CompletedAt, CreatedAt) " +
                "VALUES (@userId, @quizId, @score, @knowledgeLevel, @difficulty, @timeSpent, @completedAt, @createdAt); " +
                "SELECT LAST_INSERT_ID();", connection);

            command.Parameters.AddWithValue("@userId", performance.UserId);
            command.Parameters.AddWithValue("@quizId", performance.QuizId);
            command.Parameters.AddWithValue("@score", performance.Score ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@knowledgeLevel", performance.KnowledgeLevel ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@difficulty", performance.Difficulty ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@timeSpent", performance.TimeSpent ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@completedAt", performance.CompletedAt);
            command.Parameters.AddWithValue("@createdAt", performance.CreatedAt);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<QuizPerformance>> GetRecentQuizPerformanceAsync(int userId, int count = 10)
        {
            await EnsureQuizPerformanceTableExistsAsync();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT Id, UserId, QuizId, Score, KnowledgeLevel, Difficulty, TimeSpent, CompletedAt, CreatedAt " +
                "FROM QuizPerformance WHERE UserId = @userId " +
                "ORDER BY CompletedAt DESC LIMIT @count", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@count", count);

            var performances = new List<QuizPerformance>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                performances.Add(new QuizPerformance
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    QuizId = reader.GetInt32("QuizId"),
                    Score = reader.IsDBNull("Score") ? null : reader.GetDecimal("Score"),
                    KnowledgeLevel = reader.IsDBNull("KnowledgeLevel") ? null : reader.GetInt32("KnowledgeLevel"),
                    Difficulty = reader.IsDBNull("Difficulty") ? null : reader.GetInt32("Difficulty"),
                    TimeSpent = reader.IsDBNull("TimeSpent") ? null : reader.GetInt32("TimeSpent"),
                    CompletedAt = reader.GetDateTime("CompletedAt"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }
            return performances;
        }

        public async Task<int> CreateKnowledgeProgressionAsync(KnowledgeProgression progression)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO KnowledgeProgression (UserId, Subject, PreviousLevel, NewLevel, ChangeReason, ConfidenceScore, CreatedAt) " +
                "VALUES (@userId, @subject, @previousLevel, @newLevel, @changeReason, @confidenceScore, @createdAt); " +
                "SELECT LAST_INSERT_ID();", connection);

            command.Parameters.AddWithValue("@userId", progression.UserId);
            command.Parameters.AddWithValue("@subject", progression.Subject);
            command.Parameters.AddWithValue("@previousLevel", progression.PreviousLevel ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@newLevel", progression.NewLevel);
            command.Parameters.AddWithValue("@changeReason", progression.ChangeReason);
            command.Parameters.AddWithValue("@confidenceScore", progression.ConfidenceScore ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@createdAt", progression.CreatedAt);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<KnowledgeProgression>> GetKnowledgeProgressionAsync(int userId)
        {
            await EnsureKnowledgeProgressionTableExistsAsync();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT Id, UserId, Subject, PreviousLevel, NewLevel, ChangeReason, ConfidenceScore, ChangedAt " +
                "FROM KnowledgeProgression WHERE UserId = @userId " +
                "ORDER BY ChangedAt DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            var progressions = new List<KnowledgeProgression>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                progressions.Add(new KnowledgeProgression
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    Subject = reader.GetString("Subject"),
                    PreviousLevel = reader.IsDBNull("PreviousLevel") ? null : reader.GetInt32("PreviousLevel"),
                    NewLevel = reader.GetInt32("NewLevel"),
                    ChangeReason = reader.GetString("ChangeReason"),
                    ConfidenceScore = reader.IsDBNull("ConfidenceScore") ? null : reader.GetDecimal("ConfidenceScore"),
                    CreatedAt = reader.GetDateTime("ChangedAt")
                });
            }
            return progressions;
        }

        public async Task<UserLearningPreferences?> GetUserLearningPreferencesAsync(int userId)
        {
            await EnsureUserLearningPreferencesTableExistsAsync();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT Id, UserId, PreferredQuizLength, CustomQuestionMultiplier, PreferredDifficulty, TimeAvailable, StudyStyle, CreatedAt, UpdatedAt " +
                "FROM UserLearningPreferences WHERE UserId = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserLearningPreferences
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    PreferredQuizLength = reader.GetString("PreferredQuizLength"),
                    CustomQuestionMultiplier = reader.GetDecimal("CustomQuestionMultiplier"),
                    PreferredDifficulty = reader.GetString("PreferredDifficulty"),
                    TimeAvailable = reader.GetInt32("TimeAvailable"),
                    StudyStyle = reader.GetString("StudyStyle"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                };
            }
            return null;
        }

        public async Task<int> CreateUserLearningPreferencesAsync(UserLearningPreferences preferences)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO UserLearningPreferences (UserId, PreferredQuizLength, CustomQuestionMultiplier, PreferredDifficulty, TimeAvailable, StudyStyle, CreatedAt, UpdatedAt) " +
                "VALUES (@userId, @preferredQuizLength, @customQuestionMultiplier, @preferredDifficulty, @timeAvailable, @studyStyle, @createdAt, @updatedAt); " +
                "SELECT LAST_INSERT_ID();", connection);

            command.Parameters.AddWithValue("@userId", preferences.UserId);
            command.Parameters.AddWithValue("@preferredQuizLength", preferences.PreferredQuizLength);
            command.Parameters.AddWithValue("@customQuestionMultiplier", preferences.CustomQuestionMultiplier);
            command.Parameters.AddWithValue("@preferredDifficulty", preferences.PreferredDifficulty);
            command.Parameters.AddWithValue("@timeAvailable", preferences.TimeAvailable);
            command.Parameters.AddWithValue("@studyStyle", preferences.StudyStyle);
            command.Parameters.AddWithValue("@createdAt", preferences.CreatedAt);
            command.Parameters.AddWithValue("@updatedAt", preferences.UpdatedAt);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateUserLearningPreferencesAsync(UserLearningPreferences preferences)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "UPDATE UserLearningPreferences SET PreferredQuizLength = @preferredQuizLength, CustomQuestionMultiplier = @customQuestionMultiplier, " +
                "PreferredDifficulty = @preferredDifficulty, TimeAvailable = @timeAvailable, StudyStyle = @studyStyle, UpdatedAt = @updatedAt " +
                "WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", preferences.Id);
            command.Parameters.AddWithValue("@preferredQuizLength", preferences.PreferredQuizLength);
            command.Parameters.AddWithValue("@customQuestionMultiplier", preferences.CustomQuestionMultiplier);
            command.Parameters.AddWithValue("@preferredDifficulty", preferences.PreferredDifficulty);
            command.Parameters.AddWithValue("@timeAvailable", preferences.TimeAvailable);
            command.Parameters.AddWithValue("@studyStyle", preferences.StudyStyle);
            command.Parameters.AddWithValue("@updatedAt", preferences.UpdatedAt);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<int> CreateContentDifficultyAnalysisAsync(ContentDifficultyAnalysis analysis)
        {
            await EnsureContentDifficultyAnalysisTableExistsAsync();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO ContentDifficultyAnalysis (FileId, ComplexityScore, KnowledgeLevel, UniqueConcepts, ContentVolume, EstimatedQuestions, TimeEstimate, AnalyzedAt) " +
                "VALUES (@fileId, @complexityScore, @knowledgeLevel, @uniqueConcepts, @contentVolume, @estimatedQuestions, @timeEstimate, @analyzedAt); " +
                "SELECT LAST_INSERT_ID();", connection);

            command.Parameters.AddWithValue("@fileId", analysis.FileId);
            command.Parameters.AddWithValue("@complexityScore", analysis.ComplexityScore);
            command.Parameters.AddWithValue("@knowledgeLevel", analysis.KnowledgeLevel);
            command.Parameters.AddWithValue("@uniqueConcepts", analysis.UniqueConcepts);
            command.Parameters.AddWithValue("@contentVolume", analysis.ContentVolume);
            command.Parameters.AddWithValue("@estimatedQuestions", analysis.EstimatedQuestions);
            command.Parameters.AddWithValue("@timeEstimate", analysis.TimeEstimate);
            command.Parameters.AddWithValue("@analyzedAt", analysis.AnalyzedAt);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<GroupedFilesResponse> GetGroupedFilesAsync(int userId)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT * FROM FileUploads WHERE UserId = @userId ORDER BY UploadedAt DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            var files = new List<FileUpload>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                files.Add(new FileUpload
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    FileName = reader.GetString("FileName"),
                    FilePath = reader.GetString("FilePath"),
                    FileType = reader.GetString("FileType"),
                    FileSize = reader.GetInt64("FileSize"),
                    UploadedAt = reader.GetDateTime("UploadedAt"),
                    IsProcessed = reader.GetBoolean("IsProcessed"),
                    ProcessingStatus = reader.IsDBNull("ProcessingStatus") ? null : reader.GetString("ProcessingStatus"),
                    StudentLevel = reader.IsDBNull("StudentLevel") ? null : reader.GetString("StudentLevel"),
                    Status = reader.GetString("Status"),
                    ProcessedAt = reader.IsDBNull("ProcessedAt") ? null : reader.GetDateTime("ProcessedAt"),
                    ExtractedContent = reader.IsDBNull("ExtractedContent") ? null : reader.GetString("ExtractedContent"),
                    AutoDetectedSubject = reader.IsDBNull("AutoDetectedSubject") ? null : reader.GetString("AutoDetectedSubject"),
                    AutoDetectedTopic = reader.IsDBNull("AutoDetectedTopic") ? null : reader.GetString("AutoDetectedTopic"),
                    UserDefinedSubject = reader.IsDBNull("UserDefinedSubject") ? null : reader.GetString("UserDefinedSubject"),
                    UserDefinedTopic = reader.IsDBNull("UserDefinedTopic") ? null : reader.GetString("UserDefinedTopic"),
                    IsUserModified = reader.GetBoolean("IsUserModified"),
                    SubjectGroupId = reader.IsDBNull("SubjectGroupId") ? null : reader.GetInt32("SubjectGroupId")
                });
            }

            // Group files by subject
            var filesBySubject = new Dictionary<string, List<FileUpload>>();
            var ungroupedFiles = new List<FileUpload>();

            foreach (var file in files)
            {
                var subject = file.UserDefinedSubject ?? file.AutoDetectedSubject;
                if (string.IsNullOrEmpty(subject))
                {
                    ungroupedFiles.Add(file);
                }
                else
                {
                    if (!filesBySubject.ContainsKey(subject))
                    {
                        filesBySubject[subject] = new List<FileUpload>();
                    }
                    filesBySubject[subject].Add(file);
                }
            }

            // Get custom groups
            var customGroups = await GetSubjectGroupsByUserIdAsync(userId);

            return new GroupedFilesResponse
            {
                FilesBySubject = filesBySubject,
                CustomGroups = customGroups,
                UngroupedFiles = ungroupedFiles
            };
        }

        public async Task<List<KnowledgeProgression>> GetKnowledgeProgressionAsync(int userId, string? subject = null)
        {
            await EnsureKnowledgeProgressionTableExistsAsync();
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT * FROM KnowledgeProgression WHERE UserId = @userId " +
                (subject != null ? "AND Subject = @subject " : "") +
                "ORDER BY ChangedAt DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);
            if (subject != null)
            {
                command.Parameters.AddWithValue("@subject", subject);
            }

            var progressions = new List<KnowledgeProgression>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                progressions.Add(new KnowledgeProgression
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    Subject = reader.GetString("Subject"),
                    PreviousLevel = reader.IsDBNull("PreviousLevel") ? null : reader.GetInt32("PreviousLevel"),
                    NewLevel = reader.GetInt32("NewLevel"),
                    ChangeReason = reader.GetString("ChangeReason"),
                    ConfidenceScore = reader.IsDBNull("ConfidenceScore") ? null : reader.GetDecimal("ConfidenceScore"),
                    CreatedAt = reader.GetDateTime("ChangedAt")
                });
            }
            return progressions;
        }

        public async Task<Quiz?> GetQuizAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "SELECT * FROM Quizzes WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Quiz
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    Title = reader.GetString("Title"),
                    Questions = reader.GetString("Questions"),
                    Subject = reader.IsDBNull("Subject") ? null : reader.GetString("Subject"),
                    Topic = reader.IsDBNull("Topic") ? null : reader.GetString("Topic"),
                    Level = reader.IsDBNull("Level") ? null : reader.GetString("Level"),
                    Status = reader.IsDBNull("Status") ? null : reader.GetString("Status"),
                    SourceStudyGuideIds = reader.IsDBNull("SourceStudyGuideIds") ? null : reader.GetString("SourceStudyGuideIds"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    IsActive = reader.GetBoolean("IsActive")
                };
            }
            return null;
        }
    }
}
        