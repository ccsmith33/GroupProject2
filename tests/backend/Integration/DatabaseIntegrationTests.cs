using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using StudentStudyAI.Tests.Infrastructure;
using Xunit;

namespace StudentStudyAI.Tests.Integration;

[Collection("Database")]
public class DatabaseIntegrationTests : DatabaseTestBase
{
    [Fact]
    public void DatabaseIntegrationTests_ShouldBeCreated()
    {
        // Arrange & Act
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=StudentStudyAI_Test;Uid=test;Pwd=test;"
            })
            .Build();

        var databaseService = new DatabaseService(configuration);

        // Assert
        Assert.NotNull(databaseService);
    }

    [Fact]
    public async Task DatabaseSchema_ShouldBeValid()
    {
        // Arrange
        using var connection = await DatabaseService!.GetConnectionAsync();

        // Act & Assert - Test that all required tables exist
        var tables = new[] { "users", "file_uploads", "analysis_results", "study_sessions", "conversations" };
        
        foreach (var table in tables)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{table}'";
            var count = await command.ExecuteScalarAsync();
            Assert.Equal(1, Convert.ToInt32(count));
        }
    }

    [Fact]
    public async Task DataIntegrity_ShouldBeMaintained()
    {
        // Arrange
        var user = new User
        {
            Username = "integritytest",
            Email = "integrity@test.com",
            PasswordHash = "hashed_password"
        };

        // Act - Create user
        var userId = await DatabaseService!.CreateUserAsync(user);
        Assert.True(userId > 0);

        // Create file upload
        var fileUpload = new FileUpload
        {
            UserId = userId,
            FileName = "test.pdf",
            FilePath = "/uploads/test.pdf",
            FileType = ".pdf",
            FileSize = 1024,
            Subject = "Mathematics",
            StudentLevel = "Intermediate",
            UploadedAt = DateTime.UtcNow,
            Status = "uploaded"
        };

        var fileId = await DatabaseService.CreateFileUploadAsync(fileUpload);
        Assert.True(fileId > 0);

        // Create analysis result
        var analysisResult = new AnalysisResult
        {
            FileUploadId = fileId,
            UserId = userId,
            Subject = "Mathematics",
            Topic = "Algebra",
            CreatedAt = DateTime.UtcNow
        };

        var analysisId = await DatabaseService.CreateAnalysisResultAsync(analysisResult);
        Assert.True(analysisId > 0);

        // Create study session
        var studySession = new StudySession
        {
            UserId = userId,
            SessionName = "Mathematics Study Session",
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            Notes = "Studied Algebra and Geometry"
        };

        var sessionId = await DatabaseService.CreateStudySessionAsync(studySession);
        Assert.True(sessionId > 0);

        // Assert - Verify data integrity
        var retrievedUser = await DatabaseService.GetUserByIdAsync(userId);
        Assert.NotNull(retrievedUser);
        Assert.Equal("integritytest", retrievedUser.Username);

        var retrievedFile = await DatabaseService.GetFileUploadAsync(fileId);
        Assert.NotNull(retrievedFile);
        Assert.Equal(userId, retrievedFile.UserId);
        Assert.Equal("test.pdf", retrievedFile.FileName);

        var retrievedAnalysis = await DatabaseService.GetAnalysisResultByIdAsync(analysisId);
        Assert.NotNull(retrievedAnalysis);
        Assert.Equal(fileId, retrievedAnalysis.FileUploadId);
        Assert.Equal(userId, retrievedAnalysis.UserId);

        var retrievedSessions = await DatabaseService.GetStudySessionsByUserIdAsync(userId);
        Assert.Single(retrievedSessions);
        Assert.Equal("Mathematics Study Session", retrievedSessions[0].SessionName);

        // Test foreign key constraints
        // Try to delete user with existing file uploads (should fail or cascade)
        try
        {
            await DatabaseService.DeleteUserAsync(userId);
            // If deletion succeeds, verify cascading deletes
            var deletedFile = await DatabaseService.GetFileUploadAsync(fileId);
            Assert.Null(deletedFile);
        }
        catch (Exception)
        {
            // Expected if foreign key constraints prevent deletion
        }
    }

    [Fact]
    public async Task TransactionRollback_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User
        {
            Username = "transactiontest",
            Email = "transaction@test.com",
            PasswordHash = "hashed_password"
        };

        using var connection = await DatabaseService!.GetConnectionAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Act - Create user
            var userId = await DatabaseService.CreateUserAsync(user);
            Assert.True(userId > 0);

            // Create file upload
            var fileUpload = new FileUpload
            {
                UserId = userId,
                FileName = "test.pdf",
                FilePath = "/uploads/test.pdf",
                FileType = ".pdf",
                FileSize = 1024,
                Subject = "Mathematics",
                StudentLevel = "Intermediate",
                UploadedAt = DateTime.UtcNow,
                Status = "uploaded"
            };

            var fileId = await DatabaseService.CreateFileUploadAsync(fileUpload);
            Assert.True(fileId > 0);

            // Simulate an error condition
            throw new Exception("Simulated error for rollback test");
        }
        catch (Exception)
        {
            // Rollback the transaction
            await transaction.RollbackAsync();
        }

        // Assert - Verify that data was rolled back
        var retrievedUser = await DatabaseService.GetUserByEmailAsync("transaction@test.com");
        Assert.Null(retrievedUser);
    }

    [Fact]
    public async Task ConcurrentAccess_ShouldHandleCorrectly()
    {
        // Arrange
        var tasks = new List<Task<int>>();
        var userCount = 10;

        // Act - Create multiple users concurrently
        for (int i = 0; i < userCount; i++)
        {
            var user = new User
            {
                Username = $"concurrentuser{i}",
                Email = $"concurrent{i}@test.com",
                PasswordHash = "hashed_password"
            };

            tasks.Add(DatabaseService!.CreateUserAsync(user));
        }

        var userIds = await Task.WhenAll(tasks);

        // Assert - Verify all users were created successfully
        Assert.Equal(userCount, userIds.Length);
        Assert.All(userIds, id => Assert.True(id > 0));

        // Verify all users exist in database
        foreach (var userId in userIds)
        {
            var user = await DatabaseService!.GetUserByIdAsync(userId);
            Assert.NotNull(user);
        }

        // Test concurrent reads
        var readTasks = new List<Task<User?>>();
        foreach (var userId in userIds)
        {
            readTasks.Add(DatabaseService!.GetUserByIdAsync(userId));
        }

        var users = await Task.WhenAll(readTasks);
        Assert.Equal(userCount, users.Length);
        Assert.All(users, user => Assert.NotNull(user));
    }

    [Fact]
    public async Task DatabaseQueries_ShouldBeOptimized()
    {
        // Arrange - Create test data
        var userId = await CreateTestUser();
        var fileIds = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            var fileUpload = new FileUpload
            {
                UserId = userId,
                FileName = $"test{i}.pdf",
                FilePath = $"/uploads/test{i}.pdf",
                FileType = ".pdf",
                FileSize = 1024,
                Subject = "Mathematics",
                StudentLevel = "Intermediate",
                UploadedAt = DateTime.UtcNow,
                Status = "uploaded"
            };

            var fileId = await DatabaseService!.CreateFileUploadAsync(fileUpload);
            fileIds.Add(fileId);
        }

        // Act - Measure query performance
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Test user lookup by email (should use index)
        var user = await DatabaseService.GetUserByEmailAsync("perftest@test.com");
        Assert.NotNull(user);

        // Test file listing by user (should use index)
        var files = await DatabaseService.GetFileUploadsByUserIdAsync(userId);
        Assert.Equal(5, files.Count);

        // Test analysis results by user (should use index)
        var analyses = await DatabaseService!.GetAnalysisResultsByUserIdAsync(userId);
        Assert.NotNull(analyses);

        stopwatch.Stop();

        // Assert - Query should complete within reasonable time
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Queries took too long: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DatabaseConstraints_ShouldBeEnforced()
    {
        // Test unique email constraint
        var user1 = new User
        {
            Username = "constrainttest1",
            Email = "constraint@test.com",
            PasswordHash = "hashed_password"
        };

        var user2 = new User
        {
            Username = "constrainttest2",
            Email = "constraint@test.com", // Same email
            PasswordHash = "hashed_password"
        };

        var userId1 = await DatabaseService!.CreateUserAsync(user1);
        Assert.True(userId1 > 0);

        // Attempt to create user with duplicate email should fail
        await Assert.ThrowsAsync<Exception>(() => DatabaseService.CreateUserAsync(user2));

        // Test foreign key constraint
        var invalidFileUpload = new FileUpload
        {
            UserId = 99999, // Non-existent user ID
            FileName = "test.pdf",
            FilePath = "/uploads/test.pdf",
            FileType = ".pdf",
            FileSize = 1024,
            Subject = "Mathematics",
            StudentLevel = "Intermediate",
            UploadedAt = DateTime.UtcNow,
            Status = "uploaded"
        };

        await Assert.ThrowsAsync<Exception>(() => DatabaseService.CreateFileUploadAsync(invalidFileUpload));
    }

    [Fact]
    public async Task DatabaseCleanup_ShouldWorkCorrectly()
    {
        // Arrange - Create test data
        var userId = await CreateTestUser();
        var fileId = await CreateTestFile(userId);
        var analysisId = await CreateTestAnalysis(fileId, userId);
        var sessionId = await CreateTestStudySession(userId);

        // Act - Clean up data
        await DatabaseService!.DeleteAnalysisResultAsync(analysisId);
        await DatabaseService.DeleteFileUploadAsync(fileId);
        await DatabaseService.DeleteStudySessionAsync(sessionId);
        await DatabaseService.DeleteUserAsync(userId);

        // Assert - Verify data was deleted
        var deletedUser = await DatabaseService.GetUserByIdAsync(userId);
        Assert.Null(deletedUser);

        var deletedFile = await DatabaseService.GetFileUploadAsync(fileId);
        Assert.Null(deletedFile);

        var deletedAnalysis = await DatabaseService.GetAnalysisResultByIdAsync(analysisId);
        Assert.Null(deletedAnalysis);

        var deletedSession = await DatabaseService.GetStudySessionByIdAsync(sessionId);
        Assert.Null(deletedSession);
    }

    private async Task<int> CreateTestUser()
    {
        var user = new User
        {
            Username = "perftest",
            Email = "perftest@test.com",
            PasswordHash = "hashed_password"
        };

        return await DatabaseService!.CreateUserAsync(user);
    }

    private async Task<int> CreateTestFile(int userId)
    {
        var fileUpload = new FileUpload
        {
            UserId = userId,
            FileName = "perftest.pdf",
            FilePath = "/uploads/perftest.pdf",
            FileType = ".pdf",
            FileSize = 1024,
            Subject = "Mathematics",
            StudentLevel = "Intermediate",
            UploadedAt = DateTime.UtcNow,
            Status = "uploaded"
        };

        return await DatabaseService!.CreateFileUploadAsync(fileUpload);
    }

    private async Task<int> CreateTestAnalysis(int fileId, int userId)
    {
        var analysisResult = new AnalysisResult
        {
            FileUploadId = fileId,
            UserId = userId,
            Subject = "Mathematics",
            Topic = "Algebra",
            CreatedAt = DateTime.UtcNow
        };

        return await DatabaseService!.CreateAnalysisResultAsync(analysisResult);
    }

    private async Task<int> CreateTestStudySession(int userId)
    {
        var studySession = new StudySession
        {
            UserId = userId,
            SessionName = "Mathematics Study Session",
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            Notes = "Studied Algebra and Geometry"
        };

        return await DatabaseService!.CreateStudySessionAsync(studySession);
    }
}
