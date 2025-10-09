using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services;
using Xunit;

namespace StudentStudyAI.Tests.Services
{
    public class OpenAIServiceTests
    {
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<OpenAIService>> _mockLogger;
        private readonly OpenAIService _service;

        public OpenAIServiceTests()
        {
            _mockHttpClient = new Mock<HttpClient>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<OpenAIService>>();
            
            // Setup configuration
            _mockConfiguration.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
            _mockConfiguration.Setup(x => x["OpenAI:BaseUrl"]).Returns("https://api.openai.com/v1");
            _mockConfiguration.Setup(x => x["OpenAI:Model"]).Returns("gpt-4");
            _mockConfiguration.Setup(x => x["OpenAI:MaxTokens"]).Returns("2000");
            _mockConfiguration.Setup(x => x["OpenAI:Temperature"]).Returns("0.7");
            _mockConfiguration.Setup(x => x["OpenAI:UseMock"]).Returns("true");
            
            _service = new OpenAIService(_mockHttpClient.Object, _mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public void CountTokens_ShouldCountTokensAccurately()
        {
            // Arrange
            var text = "This is a test sentence with multiple words.";

            // Act
            var tokenCount = _service.CountTokens(text);

            // Assert
            Assert.True(tokenCount > 0);
            Assert.True(tokenCount <= text.Split(' ').Length * 2); // Rough estimate
        }

        [Fact]
        public void CountTokens_ShouldHandleEmptyText()
        {
            // Arrange
            var text = "";

            // Act
            var tokenCount = _service.CountTokens(text);

            // Assert
            Assert.Equal(0, tokenCount);
        }

        [Fact]
        public void CountTokens_ShouldHandleNullText()
        {
            // Arrange
            string? text = null;

            // Act
            var tokenCount = _service.CountTokens(text!);

            // Assert
            Assert.Equal(0, tokenCount);
        }

        [Fact]
        public void CalculateCost_ShouldCalculateCorrectCost()
        {
            // Arrange
            var inputTokens = 1000;
            var outputTokens = 500;

            // Act
            var cost = _service.CalculateCost(inputTokens, outputTokens);

            // Assert
            Assert.True(cost > 0);
        }

        [Fact]
        public void CalculateCost_ShouldHandleZeroTokens()
        {
            // Arrange
            var inputTokens = 0;
            var outputTokens = 0;

            // Act
            var cost = _service.CalculateCost(inputTokens, outputTokens);

            // Assert
            Assert.Equal(0, cost);
        }

        [Fact]
        public void CalculateCost_ShouldHandleLargeTokenCounts()
        {
            // Arrange
            var inputTokens = 10000;
            var outputTokens = 5000;

            // Act
            var cost = _service.CalculateCost(inputTokens, outputTokens);

            // Assert
            Assert.True(cost > 0);
        }

        [Fact]
        public async Task AnalyzeFileContentAsync_ShouldReturnMockDataWhenMockEnabled()
        {
            // Arrange
            var fileUpload = new StudentStudyAI.Models.FileUpload
            {
                Id = 1,
                FileName = "test.pdf",
                FilePath = "/uploads/test.pdf",
                FileType = "application/pdf",
                FileSize = 1024,
                Status = "uploaded"
            };

            // Act
            var result = await _service.AnalyzeFileContentAsync(fileUpload, "Test Subject", "Test Topic");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Summary);
        }

        [Fact]
        public async Task GenerateStudyGuideAsync_ShouldReturnMockStudyGuide()
        {
            // Arrange
            var content = "Test content for study guide";
            var contextFiles = new List<StudentStudyAI.Models.FileUpload>();
            var contextStudyGuides = new List<StudentStudyAI.Models.StudyGuide>();

            // Act
            var result = await _service.GenerateStudyGuideAsync(content, contextFiles, contextStudyGuides, "Test Subject", "Test Topic");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GenerateQuizAsync_ShouldReturnMockQuiz()
        {
            // Arrange
            var content = "Test content for quiz";
            var contextFiles = new List<StudentStudyAI.Models.FileUpload>();
            var contextStudyGuides = new List<StudentStudyAI.Models.StudyGuide>();

            // Act
            var result = await _service.GenerateQuizAsync(content, contextFiles, contextStudyGuides, "Test Subject", "Test Topic");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GenerateConversationalResponseAsync_ShouldReturnMockResponse()
        {
            // Arrange
            var question = "What is this about?";
            var contextFiles = new List<StudentStudyAI.Models.FileUpload>();
            var contextStudyGuides = new List<StudentStudyAI.Models.StudyGuide>();
            var contextConversations = new List<StudentStudyAI.Models.Conversation>();

            // Act
            var result = await _service.GenerateConversationalResponseAsync(question, contextFiles, contextStudyGuides, contextConversations, "Test Subject", "Test Topic");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task AnalyzeFileContentAsync_ShouldHandleEmptyContent()
        {
            // Arrange
            var fileUpload = new StudentStudyAI.Models.FileUpload
            {
                Id = 1,
                FileName = "empty.pdf",
                FilePath = "/uploads/empty.pdf",
                FileType = "application/pdf",
                FileSize = 0,
                Status = "uploaded"
            };

            // Act
            var result = await _service.AnalyzeFileContentAsync(fileUpload, "Test Subject", "Test Topic");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AnalyzeFileContentAsync_ShouldHandleNullContent()
        {
            // Arrange
            StudentStudyAI.Models.FileUpload? fileUpload = null;

            // Act
            var result = await _service.AnalyzeFileContentAsync(fileUpload!, "Test Subject", "Test Topic");

            // Assert
            Assert.NotNull(result);
        }
    }
}