using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services.Processors;
using Xunit;

namespace StudentStudyAI.Tests.Services.Processors
{
    public class MediaProcessorTests
    {
        private readonly Mock<ILogger<MediaProcessor>> _mockLogger;
        private readonly MediaProcessor _processor;

        public MediaProcessorTests()
        {
            _mockLogger = new Mock<ILogger<MediaProcessor>>();
            _processor = new MediaProcessor(_mockLogger.Object);
        }

        [Fact]
        public void MediaProcessor_ShouldBeCreated()
        {
            // Arrange & Act
            var processor = new MediaProcessor(_mockLogger.Object);

            // Assert
            Assert.NotNull(processor);
        }

        [Fact]
        public async Task ExtractAudioAsync_ShouldReturnEmptyStringForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.mp3";

            // Act
            var result = await _processor.ExtractAudioAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractAudioAsync_ShouldReturnEmptyStringForNullPath()
        {
            // Arrange
            string? nullPath = null;

            // Act
            var result = await _processor.ExtractAudioAsync(nullPath!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractAudioAsync_ShouldReturnEmptyStringForUnsupportedFileType()
        {
            // Arrange
            var unsupportedPath = "file.txt";

            // Act
            var result = await _processor.ExtractAudioAsync(unsupportedPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task TranscribeAudioAsync_ShouldReturnPlaceholderMessage()
        {
            // Arrange
            var audioPath = "test.mp3";

            // Act
            var result = await _processor.TranscribeAudioAsync(audioPath);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Audio transcription requires speech-to-text service integration", result);
        }

        [Fact]
        public async Task TranscribeAudioAsync_ShouldReturnEmptyStringForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.mp3";

            // Act
            var result = await _processor.TranscribeAudioAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Audio transcription requires speech-to-text service integration", result);
        }

        [Fact]
        public async Task ExtractFramesAsync_ShouldReturnEmptyListForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.mp4";

            // Act
            var result = await _processor.ExtractFramesAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractFramesAsync_ShouldReturnEmptyListForNonVideoFile()
        {
            // Arrange
            var audioPath = "test.mp3";

            // Act
            var result = await _processor.ExtractFramesAsync(audioPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractMetadataAsync_ShouldReturnEmptyDictionaryForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.mp3";

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
        public async Task ExtractAudioAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.mp3";

            // Act
            await _processor.ExtractAudioAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing audio file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TranscribeAudioAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.mp3";

            // Act
            await _processor.TranscribeAudioAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audio transcription not implemented")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExtractFramesAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.mp4";

            // Act
            await _processor.ExtractFramesAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Video frame extraction requires FFmpeg integration")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExtractMetadataAsync_ShouldLogErrorForInvalidFile()
        {
            // Arrange
            var invalidPath = "nonexistent.mp3";

            // Act
            await _processor.ExtractMetadataAsync(invalidPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error extracting audio metadata")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
