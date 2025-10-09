using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services.Processors;
using Xunit;

namespace StudentStudyAI.Tests.Services.Processors
{
    public class ImageProcessorTests
    {
        private readonly Mock<ILogger<ImageProcessor>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ImageProcessor _processor;

        public ImageProcessorTests()
        {
            _mockLogger = new Mock<ILogger<ImageProcessor>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["Tesseract:DataPath"]).Returns("tessdata");
            _processor = new ImageProcessor(_mockLogger.Object, _mockConfiguration.Object);
        }

        [Fact]
        public void ImageProcessor_ShouldBeCreated()
        {
            // Arrange & Act
            var processor = new ImageProcessor(_mockLogger.Object, _mockConfiguration.Object);

            // Assert
            Assert.NotNull(processor);
        }

        [Fact]
        public async Task ExtractTextAsync_ShouldReturnEmptyStringForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.jpg";

            // Act
            var result = await _processor.ExtractTextAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractTextAsync_ShouldReturnEmptyStringForNullPath()
        {
            // Arrange
            string? nullPath = null;

            // Act
            var result = await _processor.ExtractTextAsync(nullPath!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractTextRegionsAsync_ShouldReturnEmptyListForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.jpg";

            // Act
            var result = await _processor.ExtractTextRegionsAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractTextRegionsAsync_ShouldReturnEmptyListForNullPath()
        {
            // Arrange
            string? nullPath = null;

            // Act
            var result = await _processor.ExtractTextRegionsAsync(nullPath!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AnalyzeImageAsync_ShouldReturnEmptyDictionaryForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.jpg";

            // Act
            var result = await _processor.AnalyzeImageAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AnalyzeImageAsync_ShouldReturnEmptyDictionaryForNullPath()
        {
            // Arrange
            string? nullPath = null;

            // Act
            var result = await _processor.AnalyzeImageAsync(nullPath!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractTextAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.jpg";

            // Act
            await _processor.ExtractTextAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting text from image")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExtractTextRegionsAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.jpg";

            // Act
            await _processor.ExtractTextRegionsAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting text regions from image")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AnalyzeImageAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.jpg";

            // Act
            await _processor.AnalyzeImageAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error analyzing image")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
