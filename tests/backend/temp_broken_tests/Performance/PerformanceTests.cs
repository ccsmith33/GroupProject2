using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using StudentStudyAI.Tests.Infrastructure;
using System.Diagnostics;
using Xunit;

namespace StudentStudyAI.Tests.Performance;

[Collection("Database")]
public class PerformanceTests : DatabaseTestBase
{
    [Fact]
    public async Task FileProcessing_ShouldMeetPerformanceRequirements()
    {
        // Arrange
        var fileProcessingService = new FileProcessingService(
            Mock.Of<IPdfProcessor>(),
            Mock.Of<IWordProcessor>(),
            Mock.Of<IImageProcessor>(),
            Mock.Of<IMediaProcessor>(),
            Mock.Of<ILogger<FileProcessingService>>());

        var fileUpload = new FileUpload
        {
            Id = 1,
            FileName = "test.pdf",
            FileType = ".pdf",
            FilePath = "/uploads/test.pdf",
            FileSize = 1024 * 1024, // 1MB
            Subject = "Mathematics",
            StudentLevel = "Intermediate",
            UploadedAt = DateTime.UtcNow,
            Status = "uploaded"
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await fileProcessingService.ProcessFileAsync(fileUpload);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"File processing took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Fact]
    public async Task DatabaseQueries_ShouldMeetPerformanceRequirements()
    {
        // Arrange
        var userId = await CreateTestUser();
        var fileIds = new List<int>();

        // Create test data
        for (int i = 0; i < 10; i++)
        {
            var fileId = await CreateTestFile(userId);
            fileIds.Add(fileId);
        }

        // Act - Test user lookup performance
        var stopwatch = Stopwatch.StartNew();
        var user = await DatabaseService!.GetUserByIdAsync(userId);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(user);
        Assert.True(stopwatch.ElapsedMilliseconds < 100, 
            $"User lookup took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");

        // Act - Test file listing performance
        stopwatch.Restart();
        var files = await DatabaseService.GetFileUploadsByUserIdAsync(userId);
        stopwatch.Stop();

        // Assert
        Assert.Equal(10, files.Count);
        Assert.True(stopwatch.ElapsedMilliseconds < 200, 
            $"File listing took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");

        // Act - Test analysis results performance
        stopwatch.Restart();
        var analyses = await DatabaseService.GetAnalysisResultsByUserIdAsync(userId);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(analyses);
        Assert.True(stopwatch.ElapsedMilliseconds < 150, 
            $"Analysis listing took {stopwatch.ElapsedMilliseconds}ms, expected < 150ms");
    }

    [Fact]
    public async Task ApiEndpoints_ShouldMeetResponseTimeRequirements()
    {
        // Arrange
        var mockJwtService = new Mock<IJwtService>();
        var mockDatabaseService = new Mock<IDatabaseService>();
        var mockLogger = new Mock<ILogger<AuthController>>();

        var user = new User
        {
            Id = 1,
            Username = "perftest",
            Email = "perf@test.com",
            PasswordHash = "hashed_password"
        };

        mockDatabaseService.Setup(x => x.GetUserByEmailAsync("perf@test.com"))
            .ReturnsAsync(user);

        mockJwtService.Setup(x => x.VerifyPassword("password123", "hashed_password"))
            .Returns(true);

        mockJwtService.Setup(x => x.GenerateToken(user))
            .Returns("jwt_token");

        var controller = new AuthController(mockJwtService.Object, mockDatabaseService.Object, mockLogger.Object);

        // Act - Test login endpoint performance
        var loginRequest = new LoginRequest
        {
            Email = "perf@test.com",
            Password = "password123"
        };

        var stopwatch = Stopwatch.StartNew();
        var result = await controller.Login(loginRequest);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 200, 
            $"Login endpoint took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
    }

    [Fact]
    public async Task ConcurrentUsers_ShouldHandleLoadCorrectly()
    {
        // Arrange
        var userIds = new List<int>();
        var tasks = new List<Task<int>>();

        // Act - Create multiple users concurrently
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 50; i++)
        {
            var user = new User
            {
                Username = $"concurrentuser{i}",
                Email = $"concurrent{i}@test.com",
                PasswordHash = "hashed_password"
            };

            tasks.Add(DatabaseService!.CreateUserAsync(user));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.Equal(50, results.Length);
        Assert.All(results, id => Assert.True(id > 0));
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Concurrent user creation took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");

        // Act - Test concurrent reads
        stopwatch.Restart();
        var readTasks = new List<Task<User?>>();
        
        foreach (var userId in results)
        {
            readTasks.Add(DatabaseService.GetUserByIdAsync(userId));
        }

        var users = await Task.WhenAll(readTasks);
        stopwatch.Stop();

        // Assert
        Assert.Equal(50, users.Length);
        Assert.All(users, user => Assert.NotNull(user));
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Concurrent user reads took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
    }

    [Fact]
    public async Task MemoryUsage_ShouldNotLeak()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var userId = await CreateTestUser();

        // Act - Perform multiple operations
        for (int i = 0; i < 100; i++)
        {
            var fileId = await CreateTestFile(userId);
            var analysisId = await CreateTestAnalysis(fileId, userId);
            var sessionId = await CreateTestStudySession(userId);

            // Clean up
            await DatabaseService!.DeleteAnalysisResultAsync(analysisId);
            await DatabaseService.DeleteFileUploadAsync(fileId);
            await DatabaseService.DeleteStudySessionAsync(sessionId);
        }

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(false);

        // Assert
        var memoryIncrease = finalMemory - initialMemory;
        Assert.True(memoryIncrease < 10 * 1024 * 1024, // 10MB
            $"Memory usage increased by {memoryIncrease / 1024 / 1024}MB, expected < 10MB");
    }

    [Fact]
    public async Task FileUpload_ShouldHandleLargeFiles()
    {
        // Arrange
        var fileStorageService = new FileStorageService(
            Mock.Of<ILogger<FileStorageService>>(),
            CreateTestConfiguration());

        var largeFile = new Mock<IFormFile>();
        largeFile.Setup(f => f.FileName).Returns("largefile.pdf");
        largeFile.Setup(f => f.Length).Returns(10 * 1024 * 1024); // 10MB
        largeFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[10 * 1024 * 1024]));

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await fileStorageService.SaveFileAsync(largeFile.Object, 1);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
            $"Large file upload took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");

        // Cleanup
        if (File.Exists(result))
        {
            File.Delete(result);
        }
    }

    [Fact]
    public async Task DatabaseConnectionPool_ShouldHandleConcurrentConnections()
    {
        // Arrange
        var connectionTasks = new List<Task>();

        // Act - Create multiple concurrent connections
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 20; i++)
        {
            connectionTasks.Add(Task.Run(async () =>
            {
                using var connection = await DatabaseService!.GetConnectionAsync();
                // Simulate some work
                await Task.Delay(100);
            }));
        }

        await Task.WhenAll(connectionTasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Concurrent connections took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Fact]
    public async Task Caching_ShouldImprovePerformance()
    {
        // Arrange
        var mockAnalysisService = new Mock<AnalysisService>(
            Mock.Of<IOpenAIService>(),
            Mock.Of<IDatabaseService>(),
            Mock.Of<IContextService>(),
            Mock.Of<ILogger<AnalysisService>>());

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cachedService = new CachedAnalysisService(
            mockAnalysisService.Object,
            memoryCache,
            Mock.Of<ILogger<CachedAnalysisService>>());

        var studyGuide = new StudyGuide
        {
            Title = "Performance Test Guide",
            Sections = new List<StudySection>(),
            Summary = "Performance test summary"
        };

        mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync("test prompt", 1))
            .ReturnsAsync(studyGuide);

        // Act - First call (should be slow)
        var stopwatch1 = Stopwatch.StartNew();
        var result1 = await cachedService.GenerateStudyGuideAsync("test prompt", 1);
        stopwatch1.Stop();

        // Act - Second call (should be fast due to caching)
        var stopwatch2 = Stopwatch.StartNew();
        var result2 = await cachedService.GenerateStudyGuideAsync("test prompt", 1);
        stopwatch2.Stop();

        // Assert
        Assert.Equal(studyGuide.Title, result1.Title);
        Assert.Equal(studyGuide.Title, result2.Title);
        
        // Cached call should be significantly faster
        Assert.True(stopwatch2.ElapsedMilliseconds < stopwatch1.ElapsedMilliseconds / 2,
            $"Cached call took {stopwatch2.ElapsedMilliseconds}ms, expected < {stopwatch1.ElapsedMilliseconds / 2}ms");

        // Should only call underlying service once
        mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync("test prompt", 1), Times.Once);

        memoryCache.Dispose();
    }

    private IConfiguration CreateTestConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:Path"] = Path.GetTempPath()
            })
            .Build();
    }

    private async Task<int> CreateTestUser()
    {
        var user = new User
        {
            Username = "perftest",
            Email = "perf@test.com",
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
            FileId = fileId,
            UserId = userId,
            AnalysisType = "file_analysis",
            Result = "{\"subject\":\"Mathematics\",\"topics\":[\"Algebra\"]}",
            CreatedAt = DateTime.UtcNow
        };

        return await DatabaseService!.CreateAnalysisResultAsync(analysisResult);
    }

    private async Task<int> CreateTestStudySession(int userId)
    {
        var studySession = new StudySession
        {
            UserId = userId,
            Subject = "Mathematics",
            Duration = 60,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            TopicsStudied = new List<string> { "Algebra", "Geometry" }
        };

        return await DatabaseService!.CreateStudySessionAsync(studySession);
    }
}
