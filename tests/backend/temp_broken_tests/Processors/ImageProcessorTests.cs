using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services.Processors;
using Xunit;

namespace StudentStudyAI.Tests.Services.Processors;

public class ImageProcessorTests : IDisposable
{
    private readonly Mock<ILogger<ImageProcessor>> _mockLogger;
    private readonly ImageProcessor _imageProcessor;
    private readonly string _testImagePath;

    public ImageProcessorTests()
    {
        _mockLogger = new Mock<ILogger<ImageProcessor>>();
        _imageProcessor = new ImageProcessor(_mockLogger.Object);
        
        // Create a test image file path
        _testImagePath = Path.Combine(Path.GetTempPath(), "test.png");
    }

    [Fact]
    public async Task ExtractTextAsync_ShouldPerformOcrOnImage()
    {
        // Arrange
        await CreateTestImageFile();

        // Act
        var result = await _imageProcessor.ExtractTextAsync(_testImagePath);

        // Assert
        Assert.NotNull(result);
        // Note: This test would need an image with actual text for meaningful OCR testing
    }

    [Fact]
    public async Task ExtractMetadataAsync_ShouldExtractImageMetadata()
    {
        // Arrange
        await CreateTestImageFile();

        // Act
        var result = await _imageProcessor.ExtractMetadataAsync(_testImagePath);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("width") || result.ContainsKey("height") || result.ContainsKey("format"));
    }

    [Fact]
    public async Task ExtractTextRegionsAsync_ShouldDetectTextRegions()
    {
        // Arrange
        await CreateTestImageFile();

        // Act
        var result = await _imageProcessor.ExtractTextRegionsAsync(_testImagePath);

        // Assert
        Assert.NotNull(result);
        // Note: This test would need an image with actual text regions for meaningful testing
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleUnsupportedImageFormat()
    {
        // Arrange
        var unsupportedFilePath = Path.Combine(Path.GetTempPath(), "test.bmp");
        await File.WriteAllBytesAsync(unsupportedFilePath, new byte[] { 0x42, 0x4D });

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _imageProcessor.ProcessFileAsync(unsupportedFilePath));
    }

    [Fact]
    public async Task ExtractTextAsync_ShouldHandleFileNotFound()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.png");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _imageProcessor.ExtractTextAsync(nonExistentPath));
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleCorruptedImage()
    {
        // Arrange
        var corruptedFilePath = Path.Combine(Path.GetTempPath(), "corrupted.png");
        await File.WriteAllBytesAsync(corruptedFilePath, new byte[] { 0x00, 0x01, 0x02 });

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _imageProcessor.ProcessFileAsync(corruptedFilePath));
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".gif")]
    public async Task ProcessFileAsync_ShouldHandleSupportedImageFormats(string extension)
    {
        // Arrange
        var imagePath = Path.Combine(Path.GetTempPath(), $"test{extension}");
        await CreateTestImageFile(imagePath);

        // Act
        var result = await _imageProcessor.ProcessFileAsync(imagePath);

        // Assert
        Assert.NotNull(result);
        
        // Cleanup
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }
    }

    private async Task CreateTestImageFile(string? filePath = null)
    {
        var path = filePath ?? _testImagePath;
        
        // Create a minimal PNG file for testing
        // This is a 1x1 pixel PNG file
        var pngBytes = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, // IHDR chunk length
            0x49, 0x48, 0x44, 0x52, // IHDR
            0x00, 0x00, 0x00, 0x01, // width: 1
            0x00, 0x00, 0x00, 0x01, // height: 1
            0x08, 0x02, 0x00, 0x00, 0x00, // bit depth, color type, compression, filter, interlace
            0x90, 0x77, 0x53, 0xDE, // CRC
            0x00, 0x00, 0x00, 0x0C, // IDAT chunk length
            0x49, 0x44, 0x41, 0x54, // IDAT
            0x08, 0x99, 0x01, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, // compressed data
            0xE2, 0x21, 0xBC, 0x33, // CRC
            0x00, 0x00, 0x00, 0x00, // IEND chunk length
            0x49, 0x45, 0x4E, 0x44, // IEND
            0xAE, 0x42, 0x60, 0x82  // CRC
        };

        await File.WriteAllBytesAsync(path, pngBytes);
    }

    public void Dispose()
    {
        if (File.Exists(_testImagePath))
        {
            File.Delete(_testImagePath);
        }
    }
}
