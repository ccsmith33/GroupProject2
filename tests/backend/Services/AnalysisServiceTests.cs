using Microsoft.Extensions.Configuration;
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
            _mockContextService.Object,
            _mockDatabaseService.Object);
    }

    [Fact]
    public void AnalysisService_ShouldBeCreated()
    {
        // Arrange & Act
        var service = new AnalysisService(
            _mockOpenAIService.Object,
            _mockContextService.Object,
            _mockDatabaseService.Object);

        // Assert
        Assert.NotNull(service);
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
            Id = 1,
            FileId = 1,
            Summary = "This document covers mathematical concepts including algebra.",
            CreatedAt = DateTime.UtcNow
        };

        _mockOpenAIService.Setup(x => x.AnalyzeFileContentAsync(It.IsAny<FileUpload>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedAnalysis);

        _mockDatabaseService.Setup(x => x.CreateAnalysisResultAsync(It.IsAny<AnalysisResult>()))
            .ReturnsAsync(1);

        // Act
        var result = await _analysisService.AnalyzeFileAsync(fileUpload, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mathematics", result.Subject);
        Assert.Equal("Algebra", result.Topic);
        
        _mockOpenAIService.Verify(x => x.AnalyzeFileContentAsync(
            fileUpload, 
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Once);
        
        _mockDatabaseService.Verify(x => x.CreateAnalysisResultAsync(It.IsAny<AnalysisResult>()), Times.Once);
    }

    [Fact]
    public async Task GenerateStudyGuideAsync_ShouldGenerateStudyGuide()
    {
        // Arrange
        var userPrompt = "Create a study guide for mathematics algebra";
        var userId = 1;

        var expectedStudyGuide = new StudyGuide
        {
            Title = "Mathematics Study Guide",
            Content = "Algebra concepts and equations",
            KeyPoints = new List<string> { "Linear equations", "Quadratic equations" },
            Summary = "Comprehensive study guide for mathematics"
        };

        _mockOpenAIService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>(), It.IsAny<List<FileUpload>>(), It.IsAny<List<StudyGuide>>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Generated study guide content");

        _mockDatabaseService.Setup(x => x.CreateStudyGuideAsync(It.IsAny<StudyGuide>()))
            .ReturnsAsync(1);

        // Act
        var result = await _analysisService.GenerateStudyGuideAsync(userPrompt, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Generated study guide content", result.Content);
        
        _mockOpenAIService.Verify(x => x.GenerateStudyGuideAsync(It.IsAny<string>(), It.IsAny<List<FileUpload>>(), It.IsAny<List<StudyGuide>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GenerateQuizAsync_ShouldGenerateQuiz()
    {
        // Arrange
        var userPrompt = "Create a quiz for mathematics algebra";
        var userId = 1;

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

        _mockOpenAIService.Setup(x => x.GenerateQuizAsync(It.IsAny<string>(), It.IsAny<List<FileUpload>>(), It.IsAny<List<StudyGuide>>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Generated quiz content");

        _mockDatabaseService.Setup(x => x.CreateQuizAsync(It.IsAny<Quiz>()))
            .ReturnsAsync(1);

        // Act
        var result = await _analysisService.GenerateQuizAsync(userPrompt, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Generated quiz content", result.Title);
        
        _mockOpenAIService.Verify(x => x.GenerateQuizAsync(It.IsAny<string>(), It.IsAny<List<FileUpload>>(), It.IsAny<List<StudyGuide>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GenerateConversationalResponseAsync_ShouldGenerateResponse()
    {
        // Arrange
        var userPrompt = "Can you explain algebra?";
        var userId = 1;
        var expectedResponse = "Algebra is a branch of mathematics that deals with symbols and the rules for manipulating those symbols.";

        _mockOpenAIService.Setup(x => x.GenerateConversationalResponseAsync(It.IsAny<string>(), It.IsAny<List<FileUpload>>(), It.IsAny<List<StudyGuide>>(), It.IsAny<List<Conversation>>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        _mockDatabaseService.Setup(x => x.CreateConversationAsync(It.IsAny<Conversation>()))
            .ReturnsAsync(1);

        // Act
        var result = await _analysisService.GenerateConversationalResponseAsync(userPrompt, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
        
        _mockOpenAIService.Verify(x => x.GenerateConversationalResponseAsync(
            userPrompt, 
            It.IsAny<List<FileUpload>>(), 
            It.IsAny<List<StudyGuide>>(), 
            It.IsAny<List<Conversation>>(), 
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Once);
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

        _mockOpenAIService.Setup(x => x.AnalyzeFileContentAsync(It.IsAny<FileUpload>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("OpenAI API error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _analysisService.AnalyzeFileAsync(fileUpload, 1));
    }

    [Fact]
    public async Task GenerateStudyGuideAsync_ShouldHandleEmptyPrompt()
    {
        // Arrange
        var emptyPrompt = "";
        var userId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _analysisService.GenerateStudyGuideAsync(emptyPrompt, userId));
    }

    [Fact]
    public async Task GenerateQuizAsync_ShouldHandleInvalidUserId()
    {
        // Arrange
        var userPrompt = "Create a quiz";
        var invalidUserId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _analysisService.GenerateQuizAsync(userPrompt, invalidUserId));
    }
}
