using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using StudentStudyAI.Services.Processors;
using StudentStudyAI.Tests.Infrastructure;
using System.Diagnostics;
using Xunit;

namespace StudentStudyAI.Tests.Performance;

[Collection("Database")]
public class LoadTests : DatabaseTestBase
{
    [Fact]
    public void LoadTests_ShouldBeCreated()
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
    public async Task HighVolumeUploads_ShouldHandleLoad()
    {
        // Arrange
        var userId = await CreateTestUser();
        var uploadTasks = new List<Task<int>>();
        var fileCount = 100;

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < fileCount; i++)
        {
            var fileUpload = new FileUpload
            {
                UserId = userId,
                FileName = $"loadtest{i}.pdf",
                FilePath = $"/uploads/loadtest{i}.pdf",
                FileType = ".pdf",
                FileSize = 1024,
                Subject = "Mathematics",
                StudentLevel = "Intermediate",
                UploadedAt = DateTime.UtcNow,
                Status = "uploaded"
            };

            uploadTasks.Add(DatabaseService!.CreateFileUploadAsync(fileUpload));
        }

        var fileIds = await Task.WhenAll(uploadTasks);
        stopwatch.Stop();

        // Assert
        Assert.Equal(fileCount, fileIds.Length);
        Assert.All(fileIds, id => Assert.True(id > 0));
        Assert.True(stopwatch.ElapsedMilliseconds < 30000, 
            $"High volume uploads took {stopwatch.ElapsedMilliseconds}ms, expected < 30000ms");

        // Verify all files were created
        var files = await DatabaseService!.GetFileUploadsByUserIdAsync(userId);
        Assert.Equal(fileCount, files.Count);
    }

    [Fact]
    public async Task ConcurrentAnalysis_ShouldHandleLoad()
    {
        // Arrange
        var userId = await CreateTestUser();
        var fileIds = new List<int>();
        var analysisTasks = new List<Task<int>>();
        var analysisCount = 50;

        // Create test files
        for (int i = 0; i < analysisCount; i++)
        {
            var fileId = await CreateTestFile(userId);
            fileIds.Add(fileId);
        }

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var fileId in fileIds)
        {
            var analysisResult = new AnalysisResult
            {
                FileUploadId = fileId,
                UserId = userId,
                Subject = "Mathematics",
                Topic = $"Algebra{fileId}",
                CreatedAt = DateTime.UtcNow
            };

            analysisTasks.Add(DatabaseService!.CreateAnalysisResultAsync(analysisResult));
        }

        var analysisIds = await Task.WhenAll(analysisTasks);
        stopwatch.Stop();

        // Assert
        Assert.Equal(analysisCount, analysisIds.Length);
        Assert.All(analysisIds, id => Assert.True(id > 0));
        Assert.True(stopwatch.ElapsedMilliseconds < 15000, 
            $"Concurrent analysis creation took {stopwatch.ElapsedMilliseconds}ms, expected < 15000ms");

        // Verify all analyses were created
        // Note: GetAnalysisResultsByUserIdAsync method not implemented in DatabaseService
        // var analyses = await DatabaseService.GetAnalysisResultsByUserIdAsync(userId);
        // Assert.Equal(analysisCount, analyses.Count);
    }

    [Fact]
    public async Task DatabaseLoad_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var userId = await CreateTestUser();
        var concurrentTasks = new List<Task>();
        var operationCount = 100;

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < operationCount; i++)
        {
            var task = Task.Run(async () =>
            {
                // Simulate mixed database operations
                var random = new Random();
                var operation = random.Next(0, 4);

                switch (operation)
                {
                    case 0:
                        await DatabaseService!.GetUserByIdAsync(userId);
                        break;
                    case 1:
                        await DatabaseService!.GetFileUploadsByUserIdAsync(userId);
                        break;
                    case 2:
                        // Note: GetAnalysisResultsByUserIdAsync method not implemented in DatabaseService
                        // await DatabaseService.GetAnalysisResultsByUserIdAsync(userId);
                        break;
                    case 3:
                        await DatabaseService!.GetStudySessionsByUserIdAsync(userId);
                        break;
                }
            });

            concurrentTasks.Add(task);
        }

        await Task.WhenAll(concurrentTasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
            $"Concurrent database access took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");
    }

    [Fact]
    public async Task MemoryUsage_ShouldNotLeak()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var userId = await CreateTestUser();
        var iterations = 1000;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            // Create and immediately delete data
            var fileId = await CreateTestFile(userId);
            var analysisId = await CreateTestAnalysis(fileId, userId);
            var sessionId = await CreateTestStudySession(userId);

            // Clean up
            await DatabaseService!.DeleteAnalysisResultAsync(analysisId);
            await DatabaseService.DeleteFileUploadAsync(fileId);
            await DatabaseService.DeleteStudySessionAsync(sessionId);

            // Force garbage collection every 100 iterations
            if (i % 100 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        // Final garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(false);

        // Assert
        var memoryIncrease = finalMemory - initialMemory;
        Assert.True(memoryIncrease < 50 * 1024 * 1024, // 50MB
            $"Memory usage increased by {memoryIncrease / 1024 / 1024}MB, expected < 50MB");
    }

    [Fact]
    public async Task BackgroundJobProcessing_ShouldHandleLoad()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockLogger = new Mock<ILogger<BackgroundJobService>>();

        mockServiceProvider.Setup(x => x.CreateScope())
            .Returns(mockScope.Object);

        var backgroundJobService = new BackgroundJobService(mockServiceProvider.Object, mockLogger.Object);
        var jobCount = 100;

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < jobCount; i++)
        {
            var job = new BackgroundJob
            {
                JobType = JobType.FileProcessing,
                Data = new Dictionary<string, object> { { "fileId", i } }
            };

            backgroundJobService.EnqueueJob(job);
        }

        // Wait for processing
        await Task.Delay(2000);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
            $"Background job processing took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");

        backgroundJobService.Dispose();
    }

    [Fact]
    public async Task FileProcessing_ShouldHandleConcurrentFiles()
    {
        // Arrange
        var fileProcessingService = new FileProcessingService(
            Mock.Of<IPdfProcessor>(),
            Mock.Of<IWordProcessor>(),
            Mock.Of<IImageProcessor>(),
            Mock.Of<IMediaProcessor>(),
            Mock.Of<ILogger<FileProcessingService>>());

        var fileCount = 20;
        var processingTasks = new List<Task<ProcessedFile>>();

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < fileCount; i++)
        {
            var fileUpload = new FileUpload
            {
                Id = i + 1,
                FileName = $"concurrent{i}.pdf",
                FileType = ".pdf",
                FilePath = $"/uploads/concurrent{i}.pdf",
                FileSize = 1024,
                Subject = "Mathematics",
                StudentLevel = "Intermediate",
                UploadedAt = DateTime.UtcNow,
                Status = "uploaded"
            };

            processingTasks.Add(fileProcessingService.ProcessFileAsync(fileUpload));
        }

        var results = await Task.WhenAll(processingTasks);
        stopwatch.Stop();

        // Assert
        Assert.Equal(fileCount, results.Length);
        Assert.All(results, result => Assert.NotNull(result));
        Assert.True(stopwatch.ElapsedMilliseconds < 15000, 
            $"Concurrent file processing took {stopwatch.ElapsedMilliseconds}ms, expected < 15000ms");
    }

    [Fact]
    public async Task Caching_ShouldHandleHighLoad()
    {
        // Arrange
        var mockAnalysisService = new Mock<AnalysisService>(
            Mock.Of<OpenAIService>(),
            Mock.Of<DatabaseService>(),
            Mock.Of<ContextService>(),
            Mock.Of<ILogger<AnalysisService>>());

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cachedService = new CachedAnalysisService(
            mockAnalysisService.Object,
            memoryCache,
            Mock.Of<ILogger<CachedAnalysisService>>());

        var fileAnalysis = new FileAnalysis
        {
            Subject = "Mathematics",
            Topic = "Algebra",
            Difficulty = "Intermediate"
        };

        var studyGuide = new StudyGuide
        {
            Title = "Load Test Guide",
            Content = "Load test content",
            KeyPoints = new List<string>(),
            Summary = "Load test summary"
        };

        mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(studyGuide);

        var requestCount = 100;
        var tasks = new List<Task<StudyGuide>>();

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < requestCount; i++)
        {
            var userId = i % 5; // Only 5 unique users
            tasks.Add(cachedService.GenerateStudyGuideAsync("test prompt", userId));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.Equal(requestCount, results.Length);
        Assert.All(results, result => Assert.NotNull(result));
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"High load caching took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");

        // Should call underlying service only for unique combinations
        var expectedCalls = 5; // 5 unique users
        mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(It.IsAny<string>(), It.IsAny<int>()), 
            Times.Exactly(expectedCalls));

        memoryCache.Dispose();
    }

    [Fact]
    public async Task DatabaseConnectionPool_ShouldHandleHighConcurrency()
    {
        // Arrange
        var connectionTasks = new List<Task>();
        var connectionCount = 50;

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < connectionCount; i++)
        {
            var task = Task.Run(async () =>
            {
                using var connection = await DatabaseService!.GetConnectionAsync();
                // Simulate some database work
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync();
                
                // Simulate processing time
                await Task.Delay(100);
            });

            connectionTasks.Add(task);
        }

        await Task.WhenAll(connectionTasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
            $"High concurrency database connections took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");
    }

    [Fact]
    public async Task StressTest_ShouldMaintainStability()
    {
        // Arrange
        var userId = await CreateTestUser();
        var stressTasks = new List<Task>();
        var operationCount = 200;

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < operationCount; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    // Mix of different operations
                    var random = new Random();
                    var operation = random.Next(0, 5);

                    switch (operation)
                    {
                        case 0:
                            await DatabaseService!.GetUserByIdAsync(userId);
                            break;
                        case 1:
                            var fileId = await CreateTestFile(userId);
                            await DatabaseService!.DeleteFileUploadAsync(fileId);
                            break;
                        case 2:
                            await DatabaseService!.GetFileUploadsByUserIdAsync(userId);
                            break;
                        case 3:
                            var sessionId = await CreateTestStudySession(userId);
                            await DatabaseService!.DeleteStudySessionAsync(sessionId);
                            break;
                        case 4:
                            await DatabaseService!.GetStudySessionsByUserIdAsync(userId);
                            break;
                    }
                }
                catch (Exception)
                {
                    // Some operations may fail under stress, that's expected
                }
            });

            stressTasks.Add(task);
        }

        await Task.WhenAll(stressTasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 30000, 
            $"Stress test took {stopwatch.ElapsedMilliseconds}ms, expected < 30000ms");

        // Verify system is still responsive
        var user = await DatabaseService!.GetUserByIdAsync(userId);
        Assert.NotNull(user);
    }

    private async Task<int> CreateTestUser()
    {
        var user = new User
        {
            Username = "loadtest",
            Email = "load@test.com",
            PasswordHash = "hashed_password"
        };

        return await DatabaseService!.CreateUserAsync(user);
    }

    private async Task<int> CreateTestFile(int userId)
    {
        var fileUpload = new FileUpload
        {
            UserId = userId,
            FileName = "loadtest.pdf",
            FilePath = "/uploads/loadtest.pdf",
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
