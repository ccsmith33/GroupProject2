using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services.Processors;
using Xunit;

namespace StudentStudyAI.Tests.Services.Processors
{
    public class WordProcessorTests
    {
        private readonly Mock<ILogger<WordProcessor>> _mockLogger;
        private readonly WordProcessor _processor;

        public WordProcessorTests()
        {
            _mockLogger = new Mock<ILogger<WordProcessor>>();
            _processor = new WordProcessor(_mockLogger.Object);
        }

        [Fact]
        public void WordProcessor_ShouldBeCreated()
        {
            // Arrange & Act
            var processor = new WordProcessor(_mockLogger.Object);

            // Assert
            Assert.NotNull(processor);
        }

        [Fact]
        public async Task ExtractTextAsync_ShouldThrowExceptionForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.docx";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _processor.ExtractTextAsync(invalidPath));
        }

        [Fact]
        public async Task ExtractTextAsync_ShouldThrowExceptionForNullPath()
        {
            // Arrange
            string? nullPath = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _processor.ExtractTextAsync(nullPath!));
        }

        [Fact]
        public async Task ExtractImagesAsync_ShouldReturnEmptyListForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.docx";

            // Act
            var result = await _processor.ExtractImagesAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractImagesAsync_ShouldReturnEmptyListForNullPath()
        {
            // Arrange
            string? nullPath = null;

            // Act
            var result = await _processor.ExtractImagesAsync(nullPath!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractMetadataAsync_ShouldReturnEmptyDictionaryForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.docx";

            // Act
            var result = await _processor.ExtractMetadataAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractMetadataAsync_ShouldReturnEmptyDictionaryForNullPath()
        {
            // Arrange
            string? nullPath = null;

            // Act
            var result = await _processor.ExtractMetadataAsync(nullPath!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractTextAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.docx";

            // Act
            try
            {
                await _processor.ExtractTextAsync(invalidPath);
            }
            catch
            {
                // Expected to throw
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting text from Word document")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExtractImagesAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.docx";

            // Act
            await _processor.ExtractImagesAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting images from Word document")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExtractMetadataAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.docx";

            // Act
            await _processor.ExtractMetadataAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting metadata from Word document")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
