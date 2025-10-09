using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using Xunit;

namespace StudentStudyAI.Tests.Services;

public class AnalysisServiceTests
{
    private readonly Mock<OpenAIService> _mockOpenAIService;
    private readonly Mock<DatabaseService> _mockDatabaseService;
    private readonly Mock<ContextService> _mockContextService;
    private readonly Mock<ILogger<AnalysisService>> _mockLogger;
    private readonly AnalysisService _analysisService;

    public AnalysisServiceTests()
    {
        _mockOpenAIService = new Mock<OpenAIService>(
            Mock.Of<HttpClient>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<ILogger<OpenAIService>>());
        _mockDatabaseService = new Mock<DatabaseService>(Mock.Of<IConfiguration>());
        _mockContextService = new Mock<ContextService>(
            Mock.Of<IDatabaseService>(),
            Mock.Of<ILogger<ContextService>>());
        _mockLogger = new Mock<ILogger<AnalysisService>>();

        _analysisService = new AnalysisService(
            _mockOpenAIService.Object,
            _mockDatabaseService.Object,
            _mockContextService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeFileAsync_ShouldAnalyzeFileSuccessfully()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 1,
            FileName = "test.pdf",
            FileType = ".pdf",
            ExtractedContent = "This is test content about mathematics and algebra.",
            Subject = "Mathematics",
            StudentLevel = "Intermediate"
        };

        var expectedAnalysis = new FileAnalysis
        {
            Subject = "Mathematics",
            Topic = "Algebra",
            Difficulty = "Intermediate",
            KeyPoints = new List<string> { "Algebra", "Equations" },
            Summary = "This document covers mathematical concepts including algebra.",
            Recommendations = new List<string> { "Review basic algebra", "Practice equations" }
        };

        _mockOpenAIService.Setup(x => x.AnalyzeFileContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedAnalysis);

        _mockDatabaseService.Setup(x => x.CreateAnalysisResultAsync(It.IsAny<AnalysisResult>()))
            .ReturnsAsync(1);

        // Act
        var result = await _analysisService.AnalyzeFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mathematics", result.Subject);
        Assert.Equal("Algebra", result.Topic);
        Assert.Equal("Intermediate", result.Difficulty);
        
        _mockOpenAIService.Verify(x => x.AnalyzeFileContentAsync(
            fileUpload.ExtractedContent, 
            fileUpload.Subject, 
            fileUpload.StudentLevel), Times.Once);
        
        _mockDatabaseService.Verify(x => x.CreateAnalysisResultAsync(It.IsAny<AnalysisResult>()), Times.Once);
    }

    [Fact]
    public async Task GenerateStudyGuideAsync_ShouldGenerateStudyGuide()
    {
        // Arrange
        var fileAnalysis = new FileAnalysis
        {
            Subject = "Mathematics",
            Topic = "Algebra",
            Difficulty = "Intermediate",
            KeyPoints = new List<string> { "Equations", "Angles" }
        };

        var expectedStudyGuide = new StudyGuide
        {
            Title = "Mathematics Study Guide",
            Content = "Algebra concepts and equations",
            KeyPoints = new List<string> { "Linear equations", "Quadratic equations" },
            Summary = "Comprehensive study guide for mathematics"
        };

        _mockOpenAIService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<FileAnalysis>()))
            .ReturnsAsync(expectedStudyGuide);

        _mockDatabaseService.Setup(x => x.CreateAnalysisResultAsync(It.IsAny<AnalysisResult>()))
            .ReturnsAsync(1);

        // Act
        var result = await _analysisService.GenerateStudyGuideAsync(fileAnalysis, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mathematics Study Guide", result.Title);
        Assert.Equal("Algebra concepts and equations", result.Content);
        Assert.Contains("Linear equations", result.KeyPoints);
        
        _mockOpenAIService.Verify(x => x.GenerateStudyGuideAsync(fileAnalysis), Times.Once);
    }

    [Fact]
    public async Task GenerateQuizAsync_ShouldGenerateQuiz()
    {
        // Arrange
        var fileAnalysis = new FileAnalysis
        {
            Subject = "Mathematics",
            Topic = "Algebra",
            Difficulty = "Intermediate"
        };

        var expectedQuiz = new Quiz
        {
            Title = "Mathematics Quiz",
            QuestionsList = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What is 2 + 2?",
                    Options = new List<string> { "3", "4", "5", "6" },
                    CorrectAnswerIndex = 1,
                    Explanation = "2 + 2 equals 4"
                }
            }
        };

        _mockOpenAIService.Setup(x => x.GenerateQuizAsync(It.IsAny<FileAnalysis>()))
            .ReturnsAsync(expectedQuiz);

        _mockDatabaseService.Setup(x => x.CreateAnalysisResultAsync(It.IsAny<AnalysisResult>()))
            .ReturnsAsync(1);

        // Act
        var result = await _analysisService.GenerateQuizAsync(fileAnalysis, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mathematics Quiz", result.Title);
        Assert.Single(result.QuestionsList);
        Assert.Equal("What is 2 + 2?", result.QuestionsList[0].Question);
        Assert.Equal(1, result.QuestionsList[0].CorrectAnswerIndex);
        
        _mockOpenAIService.Verify(x => x.GenerateQuizAsync(fileAnalysis), Times.Once);
    }

    [Fact]
    public async Task GenerateConversationalResponseAsync_ShouldGenerateResponse()
    {
        // Arrange
        var conversation = new Conversation
        {
            Id = 1,
            UserId = 1,
            Prompt = "Can you explain algebra?",
            Response = "",
            Subject = "Mathematics",
            Topic = "Algebra"
        };

        var expectedResponse = "Algebra is a branch of mathematics that deals with symbols and the rules for manipulating those symbols.";

        _mockContextService.Setup(x => x.GetRelevantContextAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("Algebra is a fundamental branch of mathematics...");

        _mockOpenAIService.Setup(x => x.GenerateConversationalResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        _mockDatabaseService.Setup(x => x.AddMessageToConversationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _analysisService.GenerateConversationalResponseAsync(conversation, "Can you explain algebra?");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
        
        _mockContextService.Verify(x => x.GetRelevantContextAsync("Can you explain algebra?", 1), Times.Once);
        _mockOpenAIService.Verify(x => x.GenerateConversationalResponseAsync(
            "Can you explain algebra?", 
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeFileAsync_ShouldUseCachedResults()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 1,
            FileName = "test.pdf",
            FileType = ".pdf",
            ExtractedContent = "This is test content.",
            Subject = "Mathematics",
            StudentLevel = "Intermediate"
        };

        var cachedAnalysis = new FileAnalysis
        {
            Subject = "Mathematics",
            Topic = "Algebra",
            Difficulty = "Intermediate"
        };

        _mockDatabaseService.Setup(x => x.GetAnalysisResultByFileIdAsync(1))
            .ReturnsAsync(new AnalysisResult
            {
                Id = 1,
                FileId = 1,
                AnalysisType = "file_analysis",
                Result = System.Text.Json.JsonSerializer.Serialize(cachedAnalysis)
            });

        // Act
        var result = await _analysisService.AnalyzeFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mathematics", result.Subject);
        
        // Verify that OpenAI service was not called since we used cached results
        _mockOpenAIService.Verify(x => x.AnalyzeFileContentAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AnalyzeFileAsync_ShouldHandleOpenAIError()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 1,
            FileName = "test.pdf",
            FileType = ".pdf",
            ExtractedContent = "This is test content.",
            Subject = "Mathematics",
            StudentLevel = "Intermediate"
        };

        _mockOpenAIService.Setup(x => x.AnalyzeFileContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("OpenAI API error"));

        _mockDatabaseService.Setup(x => x.GetAnalysisResultByFileIdAsync(1))
            .ReturnsAsync((AnalysisResult?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _analysisService.AnalyzeFileAsync(fileUpload));
    }

    [Fact]
    public async Task GenerateStudyGuideAsync_ShouldHandleEmptyAnalysis()
    {
        // Arrange
        var emptyAnalysis = new FileAnalysis
        {
            Subject = "",
            Topics = new List<string>(),
            DifficultyLevel = "",
            KeyConcepts = new List<string>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _analysisService.GenerateStudyGuideAsync(emptyAnalysis, 1));
    }

    [Fact]
    public async Task GenerateQuizAsync_ShouldHandleInvalidUserId()
    {
        // Arrange
        var fileAnalysis = new FileAnalysis
        {
            Subject = "Mathematics",
            Topic = "Algebra",
            Difficulty = "Intermediate"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _analysisService.GenerateQuizAsync(fileAnalysis, 0));
    }
}
