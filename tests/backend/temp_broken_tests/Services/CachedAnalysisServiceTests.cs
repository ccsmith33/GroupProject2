using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using Xunit;

namespace StudentStudyAI.Tests.Services;

public class CachedAnalysisServiceTests : IDisposable
{
    private readonly Mock<AnalysisService> _mockAnalysisService;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<CachedAnalysisService>> _mockLogger;
    private readonly CachedAnalysisService _cachedAnalysisService;

    public CachedAnalysisServiceTests()
    {
        _mockAnalysisService = new Mock<AnalysisService>(
            Mock.Of<IOpenAIService>(),
            Mock.Of<IDatabaseService>(),
            Mock.Of<IContextService>(),
            Mock.Of<ILogger<AnalysisService>>());
        
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<CachedAnalysisService>>();
        
        _cachedAnalysisService = new CachedAnalysisService(
            _mockAnalysisService.Object,
            _memoryCache,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetCachedAnalysis_ShouldReturnCachedResult()
    {
        // Arrange
        var userId = 1;
        var prompt = "Test prompt";
        var expectedStudyGuide = new StudyGuide
        {
            Title = "Cached Study Guide",
            Sections = new List<StudySection>
            {
                new StudySection
                {
                    Title = "Test Section",
                    Content = "Test content",
                    KeyPoints = new List<string> { "Point 1", "Point 2" }
                }
            },
            Summary = "Test summary"
        };

        // First call - should cache the result
        _mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(prompt, userId))
            .ReturnsAsync(expectedStudyGuide);

        // Act - First call
        var result1 = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt, userId);

        // Act - Second call (should use cache)
        var result2 = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt, userId);

        // Assert
        Assert.Equal(expectedStudyGuide.Title, result1.Title);
        Assert.Equal(expectedStudyGuide.Title, result2.Title);
        
        // Should only call the underlying service once
        _mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(prompt, userId), Times.Once);
    }

    [Fact]
    public async Task GetCachedAnalysis_ShouldCacheNewResult()
    {
        // Arrange
        var userId = 1;
        var prompt = "New prompt";
        var expectedStudyGuide = new StudyGuide
        {
            Title = "New Study Guide",
            Sections = new List<StudySection>(),
            Summary = "New summary"
        };

        _mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(prompt, userId))
            .ReturnsAsync(expectedStudyGuide);

        // Act
        var result = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt, userId);

        // Assert
        Assert.Equal(expectedStudyGuide.Title, result.Title);
        _mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(prompt, userId), Times.Once);
    }

    [Fact]
    public async Task GetCachedAnalysis_ShouldExpireOldResults()
    {
        // Arrange
        var userId = 1;
        var prompt = "Expiring prompt";
        var expectedStudyGuide = new StudyGuide
        {
            Title = "Expiring Study Guide",
            Sections = new List<StudySection>(),
            Summary = "Expiring summary"
        };

        // Create a service with short cache expiration
        var shortCache = new MemoryCache(new MemoryCacheOptions
        {
            Clock = new TestClock(),
            ExpirationScanFrequency = TimeSpan.FromMilliseconds(100)
        });

        var service = new CachedAnalysisService(
            _mockAnalysisService.Object,
            shortCache,
            _mockLogger.Object);

        _mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(prompt, userId))
            .ReturnsAsync(expectedStudyGuide);

        // Act - First call
        var result1 = await service.GenerateStudyGuideAsync(prompt, userId);

        // Wait for cache to expire
        await Task.Delay(200);

        // Act - Second call (should not use cache)
        var result2 = await service.GenerateStudyGuideAsync(prompt, userId);

        // Assert
        Assert.Equal(expectedStudyGuide.Title, result1.Title);
        Assert.Equal(expectedStudyGuide.Title, result2.Title);
        
        // Should call the underlying service twice
        _mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(prompt, userId), Times.Exactly(2));
    }

    [Fact]
    public async Task InvalidateCache_ShouldClearCache()
    {
        // Arrange
        var userId = 1;
        var prompt = "Cache invalidation test";
        var expectedStudyGuide = new StudyGuide
        {
            Title = "Cache Test Study Guide",
            Sections = new List<StudySection>(),
            Summary = "Cache test summary"
        };

        _mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(prompt, userId))
            .ReturnsAsync(expectedStudyGuide);

        // Act - First call (should cache)
        var result1 = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt, userId);

        // Act - Invalidate cache (simulate by clearing memory cache)
        _memoryCache.Remove($"study_guide_{userId}_{prompt.GetHashCode()}");

        // Act - Second call (should not use cache)
        var result2 = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt, userId);

        // Assert
        Assert.Equal(expectedStudyGuide.Title, result1.Title);
        Assert.Equal(expectedStudyGuide.Title, result2.Title);
        
        // Should call the underlying service twice
        _mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(prompt, userId), Times.Exactly(2));
    }

    [Fact]
    public async Task GenerateQuizAsync_ShouldUseCache()
    {
        // Arrange
        var userId = 1;
        var prompt = "Quiz test prompt";
        var expectedQuiz = new Quiz
        {
            Title = "Cached Quiz",
            Questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "Test question?",
                    Options = new List<string> { "A", "B", "C", "D" },
                    CorrectAnswer = 0,
                    Explanation = "Test explanation"
                }
            },
            TotalQuestions = 1
        };

        _mockAnalysisService.Setup(x => x.GenerateQuizAsync(prompt, userId))
            .ReturnsAsync(expectedQuiz);

        // Act - First call
        var result1 = await _cachedAnalysisService.GenerateQuizAsync(prompt, userId);

        // Act - Second call (should use cache)
        var result2 = await _cachedAnalysisService.GenerateQuizAsync(prompt, userId);

        // Assert
        Assert.Equal(expectedQuiz.Title, result1.Title);
        Assert.Equal(expectedQuiz.Title, result2.Title);
        
        // Should only call the underlying service once
        _mockAnalysisService.Verify(x => x.GenerateQuizAsync(prompt, userId), Times.Once);
    }

    [Fact]
    public async Task GenerateConversationalResponseAsync_ShouldUseCache()
    {
        // Arrange
        var userId = 1;
        var prompt = "Conversation test prompt";
        var context = "Test context";
        var expectedResponse = "Cached conversational response";

        _mockAnalysisService.Setup(x => x.GenerateConversationalResponseAsync(prompt, context, userId))
            .ReturnsAsync(expectedResponse);

        // Act - First call
        var result1 = await _cachedAnalysisService.GenerateConversationalResponseAsync(prompt, context, userId);

        // Act - Second call (should use cache)
        var result2 = await _cachedAnalysisService.GenerateConversationalResponseAsync(prompt, context, userId);

        // Assert
        Assert.Equal(expectedResponse, result1);
        Assert.Equal(expectedResponse, result2);
        
        // Should only call the underlying service once
        _mockAnalysisService.Verify(x => x.GenerateConversationalResponseAsync(prompt, context, userId), Times.Once);
    }

    [Fact]
    public async Task DifferentUsers_ShouldHaveSeparateCaches()
    {
        // Arrange
        var prompt = "Shared prompt";
        var studyGuide1 = new StudyGuide
        {
            Title = "User 1 Study Guide",
            Sections = new List<StudySection>(),
            Summary = "User 1 summary"
        };

        var studyGuide2 = new StudyGuide
        {
            Title = "User 2 Study Guide",
            Sections = new List<StudySection>(),
            Summary = "User 2 summary"
        };

        _mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(prompt, 1))
            .ReturnsAsync(studyGuide1);
        _mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(prompt, 2))
            .ReturnsAsync(studyGuide2);

        // Act
        var result1 = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt, 1);
        var result2 = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt, 2);

        // Assert
        Assert.Equal("User 1 Study Guide", result1.Title);
        Assert.Equal("User 2 Study Guide", result2.Title);
        
        // Should call the underlying service twice (once for each user)
        _mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(prompt, 1), Times.Once);
        _mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(prompt, 2), Times.Once);
    }

    [Fact]
    public async Task DifferentPrompts_ShouldHaveSeparateCaches()
    {
        // Arrange
        var userId = 1;
        var prompt1 = "Prompt 1";
        var prompt2 = "Prompt 2";
        var studyGuide1 = new StudyGuide
        {
            Title = "Study Guide 1",
            Sections = new List<StudySection>(),
            Summary = "Summary 1"
        };

        var studyGuide2 = new StudyGuide
        {
            Title = "Study Guide 2",
            Sections = new List<StudySection>(),
            Summary = "Summary 2"
        };

        _mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(prompt1, userId))
            .ReturnsAsync(studyGuide1);
        _mockAnalysisService.Setup(x => x.GenerateStudyGuideAsync(prompt2, userId))
            .ReturnsAsync(studyGuide2);

        // Act
        var result1 = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt1, userId);
        var result2 = await _cachedAnalysisService.GenerateStudyGuideAsync(prompt2, userId);

        // Assert
        Assert.Equal("Study Guide 1", result1.Title);
        Assert.Equal("Study Guide 2", result2.Title);
        
        // Should call the underlying service twice (once for each prompt)
        _mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(prompt1, userId), Times.Once);
        _mockAnalysisService.Verify(x => x.GenerateStudyGuideAsync(prompt2, userId), Times.Once);
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
    }

    // Test clock for controlling cache expiration
    private class TestClock : Microsoft.Extensions.Internal.ISystemClock
    {
        private DateTimeOffset _utcNow = DateTimeOffset.UtcNow;

        public DateTimeOffset UtcNow
        {
            get => _utcNow;
            set => _utcNow = value;
        }
    }
}
