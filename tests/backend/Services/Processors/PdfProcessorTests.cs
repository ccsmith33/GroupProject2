using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services.Processors;
using Xunit;

namespace StudentStudyAI.Tests.Services.Processors
{
    public class PdfProcessorTests
    {
        private readonly Mock<ILogger<PdfProcessor>> _mockLogger;
        private readonly PdfProcessor _processor;

        public PdfProcessorTests()
        {
            _mockLogger = new Mock<ILogger<PdfProcessor>>();
            _processor = new PdfProcessor(_mockLogger.Object);
        }

        [Fact]
        public void PdfProcessor_ShouldBeCreated()
        {
            // Arrange & Act
            var processor = new PdfProcessor(_mockLogger.Object);

            // Assert
            Assert.NotNull(processor);
        }

        [Fact]
        public async Task ExtractTextAsync_ShouldThrowExceptionForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.pdf";

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(() => _processor.ExtractTextAsync(invalidPath));
        }

        [Fact]
        public async Task ExtractTextAsync_ShouldThrowExceptionForNullPath()
        {
            // Arrange
            string? nullPath = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _processor.ExtractTextAsync(nullPath!));
        }

        [Fact]
        public async Task ExtractImagesAsync_ShouldReturnEmptyListForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.pdf";

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
            var invalidPath = "nonexistent.pdf";

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
            var invalidPath = "nonexistent.pdf";

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting text from PDF")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExtractImagesAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.pdf";

            // Act
            await _processor.ExtractImagesAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting images from PDF")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExtractMetadataAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.pdf";

            // Act
            await _processor.ExtractMetadataAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting metadata from PDF")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
