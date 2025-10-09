using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using StudentStudyAI.Services.Processors;
using Xunit;

namespace StudentStudyAI.Tests.Services;

public class FileProcessingServiceTests
{
    private readonly Mock<IPdfProcessor> _mockPdfProcessor;
    private readonly Mock<IWordProcessor> _mockWordProcessor;
    private readonly Mock<IImageProcessor> _mockImageProcessor;
    private readonly Mock<IMediaProcessor> _mockMediaProcessor;
    private readonly Mock<ILogger<FileProcessingService>> _mockLogger;
    private readonly FileProcessingService _fileProcessingService;

    public FileProcessingServiceTests()
    {
        _mockPdfProcessor = new Mock<IPdfProcessor>();
        _mockWordProcessor = new Mock<IWordProcessor>();
        _mockImageProcessor = new Mock<IImageProcessor>();
        _mockMediaProcessor = new Mock<IMediaProcessor>();
        _mockLogger = new Mock<ILogger<FileProcessingService>>();

        _fileProcessingService = new FileProcessingService(
            _mockPdfProcessor.Object,
            _mockWordProcessor.Object,
            _mockImageProcessor.Object,
            _mockMediaProcessor.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldRouteToPdfProcessor()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 1,
            FileName = "test.pdf",
            FileType = "application/pdf",
            FilePath = "/uploads/test.pdf"
        };

        var expectedText = "Extracted PDF text";
        var expectedImages = new List<string> { "image1.jpg", "image2.jpg" };
        var expectedMetadata = new Dictionary<string, object> { { "pages", 5 } };

        _mockPdfProcessor.Setup(x => x.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedText);
        _mockPdfProcessor.Setup(x => x.ExtractImagesAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedImages);
        _mockPdfProcessor.Setup(x => x.ExtractMetadataAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.FileId);
        Assert.Equal("test.pdf", result.FileName);
        Assert.Equal("application/pdf", result.FileType);
        Assert.Equal("completed", result.Status);
        Assert.Equal(expectedText, result.ExtractedText);
        Assert.Equal(expectedImages, result.ExtractedImages);
        Assert.Equal(expectedMetadata, result.Metadata);

        _mockPdfProcessor.Verify(x => x.ExtractTextAsync(fileUpload.FilePath), Times.Once);
        _mockPdfProcessor.Verify(x => x.ExtractImagesAsync(fileUpload.FilePath), Times.Once);
        _mockPdfProcessor.Verify(x => x.ExtractMetadataAsync(fileUpload.FilePath), Times.Once);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldRouteToWordProcessor()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 2,
            FileName = "test.docx",
            FileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FilePath = "/uploads/test.docx"
        };

        var expectedText = "Extracted Word text";
        var expectedImages = new List<string> { "word_image1.jpg" };
        var expectedMetadata = new Dictionary<string, object> { { "wordCount", 150 } };

        _mockWordProcessor.Setup(x => x.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedText);
        _mockWordProcessor.Setup(x => x.ExtractImagesAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedImages);
        _mockWordProcessor.Setup(x => x.ExtractMetadataAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.FileId);
        Assert.Equal("test.docx", result.FileName);
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", result.FileType);
        Assert.Equal("completed", result.Status);
        Assert.Equal(expectedText, result.ExtractedText);
        Assert.Equal(expectedImages, result.ExtractedImages);
        Assert.Equal(expectedMetadata, result.Metadata);

        _mockWordProcessor.Verify(x => x.ExtractTextAsync(fileUpload.FilePath), Times.Once);
        _mockWordProcessor.Verify(x => x.ExtractImagesAsync(fileUpload.FilePath), Times.Once);
        _mockWordProcessor.Verify(x => x.ExtractMetadataAsync(fileUpload.FilePath), Times.Once);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldRouteToImageProcessor()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 3,
            FileName = "test.jpg",
            FileType = "image/jpeg",
            FilePath = "/uploads/test.jpg"
        };

        var expectedText = "Extracted OCR text";
        var expectedImages = new List<string> { "test.jpg" };
        var expectedMetadata = new Dictionary<string, object> { { "width", 1920 }, { "height", 1080 } };

        _mockImageProcessor.Setup(x => x.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedText);
        _mockImageProcessor.Setup(x => x.ExtractTextRegionsAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedImages);
        _mockImageProcessor.Setup(x => x.AnalyzeImageAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.FileId);
        Assert.Equal("test.jpg", result.FileName);
        Assert.Equal("image/jpeg", result.FileType);
        Assert.Equal("completed", result.Status);
        Assert.Equal(expectedText, result.ExtractedText);
        Assert.Equal(expectedImages, result.ExtractedImages);
        Assert.Equal(expectedMetadata, result.Metadata);

        _mockImageProcessor.Verify(x => x.ExtractTextAsync(fileUpload.FilePath), Times.Once);
        _mockImageProcessor.Verify(x => x.ExtractTextRegionsAsync(fileUpload.FilePath), Times.Once);
        _mockImageProcessor.Verify(x => x.AnalyzeImageAsync(fileUpload.FilePath), Times.Once);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldRouteToMediaProcessor()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 4,
            FileName = "test.mp4",
            FileType = "video/mp4",
            FilePath = "/uploads/test.mp4"
        };

        var expectedText = "Transcribed audio text";
        var expectedImages = new List<string> { "frame1.jpg", "frame2.jpg" };
        var expectedMetadata = new Dictionary<string, object> { { "duration", 120 }, { "fps", 30 } };

        _mockMediaProcessor.Setup(x => x.TranscribeAudioAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedText);
        _mockMediaProcessor.Setup(x => x.ExtractFramesAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedImages);
        _mockMediaProcessor.Setup(x => x.ExtractMetadataAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.FileId);
        Assert.Equal("test.mp4", result.FileName);
        Assert.Equal("video/mp4", result.FileType);
        Assert.Equal("completed", result.Status);
        Assert.Equal(expectedText, result.ExtractedText);
        Assert.Equal(expectedImages, result.ExtractedImages);
        Assert.Equal(expectedMetadata, result.Metadata);

        _mockMediaProcessor.Verify(x => x.TranscribeAudioAsync(fileUpload.FilePath), Times.Once);
        _mockMediaProcessor.Verify(x => x.ExtractFramesAsync(fileUpload.FilePath), Times.Once);
        _mockMediaProcessor.Verify(x => x.ExtractMetadataAsync(fileUpload.FilePath), Times.Once);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleUnsupportedFileType()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 5,
            FileName = "test.xyz",
            FileType = "application/unknown",
            FilePath = "/uploads/test.xyz"
        };

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.FileId);
        Assert.Equal("test.xyz", result.FileName);
        Assert.Equal("application/unknown", result.FileType);
        Assert.Equal("unsupported", result.Status);
        Assert.Empty(result.ExtractedText);
        Assert.Empty(result.ExtractedImages);
        Assert.Empty(result.Metadata);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleProcessingErrors()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 6,
            FileName = "corrupted.pdf",
            FileType = "application/pdf",
            FilePath = "/uploads/corrupted.pdf"
        };

        _mockPdfProcessor.Setup(x => x.ExtractTextAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("PDF processing failed"));

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.FileId);
        Assert.Equal("corrupted.pdf", result.FileName);
        Assert.Equal("error", result.Status);
        Assert.Contains("PDF processing failed", result.ErrorMessage);
        Assert.Empty(result.ExtractedImages);
        Assert.Empty(result.Metadata);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldSetProcessingStatus()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 7,
            FileName = "test.pdf",
            FileType = "application/pdf",
            FilePath = "/uploads/test.pdf"
        };

        _mockPdfProcessor.Setup(x => x.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("Test text");

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.Equal("completed", result.Status);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldSetCompletedStatus()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 8,
            FileName = "test.pdf",
            FileType = "application/pdf",
            FilePath = "/uploads/test.pdf"
        };

        _mockPdfProcessor.Setup(x => x.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("Test text");
        _mockPdfProcessor.Setup(x => x.ExtractImagesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<string>());
        _mockPdfProcessor.Setup(x => x.ExtractMetadataAsync(It.IsAny<string>()))
            .ReturnsAsync(new Dictionary<string, object>());

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.Equal("completed", result.Status);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldSetErrorStatusOnException()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 9,
            FileName = "error.pdf",
            FileType = "application/pdf",
            FilePath = "/uploads/error.pdf"
        };

        _mockPdfProcessor.Setup(x => x.ExtractTextAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Processing error"));

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.Equal("error", result.Status);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldLogProcessingStart()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 10,
            FileName = "test.pdf",
            FileType = "application/pdf",
            FilePath = "/uploads/test.pdf"
        };

        _mockPdfProcessor.Setup(x => x.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("Test text");

        // Act
        await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting file processing for: test.pdf")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".png", "image/png")]
    [InlineData(".mp4", "video/mp4")]
    [InlineData(".mp3", "audio/mpeg")]
    [InlineData(".wav", "audio/wav")]
    public async Task ProcessFileAsync_ShouldRouteCorrectlyByExtension(string extension, string expectedType)
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            Id = 11,
            FileName = $"test{extension}",
            FileType = expectedType,
            FilePath = $"/uploads/test{extension}"
        };

        // Act
        var result = await _fileProcessingService.ProcessFileAsync(fileUpload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedType, result.FileType);
    }
}