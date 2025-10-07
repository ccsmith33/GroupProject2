using MySql.Data.MySqlClient;
using StudentStudyAI.Models;
using System.Data;

namespace StudentStudyAI.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("DefaultConnection string is missing from appsettings.json. Please add your MySQL connection string.");
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
                    ProcessingStatus = reader.IsDBNull("ProcessingStatus") ? null : reader.GetString("ProcessingStatus")
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
                    ProcessingStatus = reader.IsDBNull("ProcessingStatus") ? null : reader.GetString("ProcessingStatus")
                });
            }
            return files;
        }

        public async Task<int> CreateFileUploadAsync(FileUpload file)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO FileUploads (UserId, FileName, FilePath, FileType, FileSize, UploadedAt, IsProcessed, ProcessingStatus) " +
                "VALUES (@userId, @fileName, @filePath, @fileType, @fileSize, @uploadedAt, @isProcessed, @processingStatus); " +
                "SELECT LAST_INSERT_ID();", connection);
            
            command.Parameters.AddWithValue("@userId", file.UserId);
            command.Parameters.AddWithValue("@fileName", file.FileName);
            command.Parameters.AddWithValue("@filePath", file.FilePath);
            command.Parameters.AddWithValue("@fileType", file.FileType);
            command.Parameters.AddWithValue("@fileSize", file.FileSize);
            command.Parameters.AddWithValue("@uploadedAt", file.UploadedAt);
            command.Parameters.AddWithValue("@isProcessed", file.IsProcessed);
            command.Parameters.AddWithValue("@processingStatus", file.ProcessingStatus);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Analysis operations
        public async Task<int> CreateAnalysisResultAsync(AnalysisResult analysis)
        {
            using var connection = await GetConnectionAsync();
            var command = new MySqlCommand(
                "INSERT INTO AnalysisResults (OverallScore, Feedback, StudyPlan, CreatedAt) " +
                "VALUES (@overallScore, @feedback, @studyPlan, @createdAt); " +
                "SELECT LAST_INSERT_ID();", connection);
            
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
            using var connection = await GetConnectionAsync();
            
            // Create tables if they don't exist
            var createTablesScript = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(50) NOT NULL UNIQUE,
                    Email VARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash VARCHAR(255) NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    LastLoginAt DATETIME NOT NULL,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE
                );

                CREATE TABLE IF NOT EXISTS FileUploads (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    FileName VARCHAR(255) NOT NULL,
                    FilePath VARCHAR(500) NOT NULL,
                    FileType VARCHAR(10) NOT NULL,
                    FileSize BIGINT NOT NULL,
                    UploadedAt DATETIME NOT NULL,
                    IsProcessed BOOLEAN NOT NULL DEFAULT FALSE,
                    ProcessingStatus VARCHAR(50),
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS AnalysisResults (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    OverallScore DECIMAL(5,2) NOT NULL,
                    Feedback TEXT NOT NULL,
                    StudyPlan TEXT NOT NULL,
                    CreatedAt DATETIME NOT NULL
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

            var command = new MySqlCommand(createTablesScript, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
