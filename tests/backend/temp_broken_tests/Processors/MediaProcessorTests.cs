using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services.Processors;
using Xunit;

namespace StudentStudyAI.Tests.Services.Processors;

public class MediaProcessorTests : IDisposable
{
    private readonly Mock<ILogger<MediaProcessor>> _mockLogger;
    private readonly MediaProcessor _mediaProcessor;
    private readonly string _testAudioPath;
    private readonly string _testVideoPath;

    public MediaProcessorTests()
    {
        _mockLogger = new Mock<ILogger<MediaProcessor>>();
        _mediaProcessor = new MediaProcessor(_mockLogger.Object);
        
        // Create test media file paths
        _testAudioPath = Path.Combine(Path.GetTempPath(), "test.mp3");
        _testVideoPath = Path.Combine(Path.GetTempPath(), "test.mp4");
    }

    [Fact]
    public async Task TranscribeAudioAsync_ShouldTranscribeAudioFile()
    {
        // Arrange
        await CreateTestAudioFile();

        // Act
        var result = await _mediaProcessor.TranscribeAudioAsync(_testAudioPath);

        // Assert
        Assert.NotNull(result);
        // Note: This test would need actual audio content for meaningful transcription testing
    }

    [Fact]
    public async Task ExtractFramesAsync_ShouldExtractVideoFrames()
    {
        // Arrange
        await CreateTestVideoFile();

        // Act
        var result = await _mediaProcessor.ExtractFramesAsync(_testVideoPath);

        // Assert
        Assert.NotNull(result);
        // Note: This test would need actual video content for meaningful frame extraction
    }

    [Fact]
    public async Task ExtractMetadataAsync_ShouldExtractMediaMetadata()
    {
        // Arrange
        await CreateTestAudioFile();

        // Act
        var result = await _mediaProcessor.ExtractMetadataAsync(_testAudioPath);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("duration") || result.ContainsKey("format") || result.ContainsKey("bitrate"));
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleUnsupportedMediaFormat()
    {
        // Arrange
        var unsupportedFilePath = Path.Combine(Path.GetTempPath(), "test.wav");
        await File.WriteAllBytesAsync(unsupportedFilePath, new byte[] { 0x52, 0x49, 0x46, 0x46 });

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _mediaProcessor.ProcessFileAsync(unsupportedFilePath));
    }

    [Fact]
    public async Task TranscribeAudioAsync_ShouldHandleFileNotFound()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.mp3");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _mediaProcessor.TranscribeAudioAsync(nonExistentPath));
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleCorruptedMediaFile()
    {
        // Arrange
        var corruptedFilePath = Path.Combine(Path.GetTempPath(), "corrupted.mp3");
        await File.WriteAllBytesAsync(corruptedFilePath, new byte[] { 0x00, 0x01, 0x02 });

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _mediaProcessor.ProcessFileAsync(corruptedFilePath));
    }

    [Theory]
    [InlineData(".mp3")]
    [InlineData(".wav")]
    [InlineData(".mp4")]
    [InlineData(".avi")]
    public async Task ProcessFileAsync_ShouldHandleSupportedMediaFormats(string extension)
    {
        // Arrange
        var mediaPath = Path.Combine(Path.GetTempPath(), $"test{extension}");
        
        if (extension == ".mp3" || extension == ".wav")
        {
            await CreateTestAudioFile(mediaPath);
        }
        else
        {
            await CreateTestVideoFile(mediaPath);
        }

        // Act
        var result = await _mediaProcessor.ProcessFileAsync(mediaPath);

        // Assert
        Assert.NotNull(result);
        
        // Cleanup
        if (File.Exists(mediaPath))
        {
            File.Delete(mediaPath);
        }
    }

    [Fact]
    public async Task ExtractFramesAsync_ShouldHandleVideoFileNotFound()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.mp4");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _mediaProcessor.ExtractFramesAsync(nonExistentPath));
    }

    private async Task CreateTestAudioFile(string? filePath = null)
    {
        var path = filePath ?? _testAudioPath;
        
        // Create a minimal MP3 file for testing
        // This is a very basic MP3 header
        var mp3Bytes = new byte[]
        {
            0xFF, 0xFB, 0x90, 0x00, // MP3 frame header
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // minimal data
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        await File.WriteAllBytesAsync(path, mp3Bytes);
    }

    private async Task CreateTestVideoFile(string? filePath = null)
    {
        var path = filePath ?? _testVideoPath;
        
        // Create a minimal MP4 file for testing
        // This is a very basic MP4 container
        var mp4Bytes = new byte[]
        {
            0x00, 0x00, 0x00, 0x20, // box size
            0x66, 0x74, 0x79, 0x70, // ftyp
            0x69, 0x73, 0x6F, 0x6D, // major brand
            0x00, 0x00, 0x02, 0x00, // minor version
            0x69, 0x73, 0x6F, 0x6D, // compatible brand
            0x69, 0x73, 0x6F, 0x32, // compatible brand
            0x6D, 0x70, 0x34, 0x31  // compatible brand
        };

        await File.WriteAllBytesAsync(path, mp4Bytes);
    }

    public void Dispose()
    {
        if (File.Exists(_testAudioPath))
        {
            File.Delete(_testAudioPath);
        }
        
        if (File.Exists(_testVideoPath))
        {
            File.Delete(_testVideoPath);
        }
    }
}
