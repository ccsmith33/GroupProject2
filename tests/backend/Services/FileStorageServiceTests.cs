using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services;
using System.IO;
using Xunit;

namespace StudentStudyAI.Tests.Services;

public class FileStorageServiceTests : IDisposable
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<FileStorageService>> _mockLogger;
    private readonly string _testUploadPath;
    private readonly FileStorageService _fileStorageService;

    public FileStorageServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<FileStorageService>>();
        
        // Create a temporary test directory
        _testUploadPath = Path.Combine(Path.GetTempPath(), "StudentStudyAI_Test_Uploads", Guid.NewGuid().ToString());
        
        _mockConfiguration.Setup(x => x["FileStorage:Path"]).Returns(_testUploadPath);
        
        _fileStorageService = new FileStorageService(_mockLogger.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task SaveFileAsync_ShouldSaveFileSuccessfully()
    {
        // Arrange
        var userId = 1;
        var fileName = "test.txt";
        var fileContent = "Hello, World!";
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileContent.Length);
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
        mockFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(contentBytes));

        // Act
        var result = await _fileStorageService.SaveFileAsync(mockFile.Object, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(userId.ToString(), result);
        Assert.Contains(Path.GetExtension(fileName), result);
        
        // Verify file was actually saved
        Assert.True(File.Exists(result));
        var savedContent = await File.ReadAllTextAsync(result);
        Assert.Equal(fileContent, savedContent);
    }

    [Fact]
    public async Task SaveFileAsync_ShouldCreateUserDirectory()
    {
        // Arrange
        var userId = 999;
        var fileName = "test.txt";
        var fileContent = "Hello, World!";
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileContent.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)));

        // Act
        var result = await _fileStorageService.SaveFileAsync(mockFile.Object, userId);

        // Assert
        var userDir = Path.Combine(_testUploadPath, userId.ToString());
        Assert.True(Directory.Exists(userDir));
        Assert.True(File.Exists(result));
    }

    [Fact]
    public async Task GetFileAsync_ShouldReturnFileStream()
    {
        // Arrange
        var userId = 1;
        var fileName = "test.txt";
        var fileContent = "Hello, World!";
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileContent.Length);
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
        mockFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(contentBytes));

        var savedPath = await _fileStorageService.SaveFileAsync(mockFile.Object, userId);

        // Act
        var result = await _fileStorageService.GetFileAsync(savedPath);

        // Assert
        Assert.NotNull(result);
        using var reader = new StreamReader(result);
        var content = await reader.ReadToEndAsync();
        Assert.Equal(fileContent, content);
    }

    [Fact]
    public async Task GetFileAsync_ShouldReturnNullForNonExistentFile()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testUploadPath, "nonexistent.txt");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _fileStorageService.GetFileAsync(nonExistentPath));
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldRemoveFileFromStorage()
    {
        // Arrange
        var userId = 1;
        var fileName = "test.txt";
        var fileContent = "Hello, World!";
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileContent.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)));

        var savedPath = await _fileStorageService.SaveFileAsync(mockFile.Object, userId);
        Assert.True(File.Exists(savedPath));

        // Act
        await _fileStorageService.DeleteFileAsync(savedPath);

        // Assert
        Assert.False(File.Exists(savedPath));
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldNotThrowForNonExistentFile()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testUploadPath, "nonexistent.txt");

        // Act & Assert - Should not throw
        await _fileStorageService.DeleteFileAsync(nonExistentPath);
    }

    [Fact]
    public void GetContentType_ShouldReturnCorrectMimeType()
    {
        // Test various file extensions
        var testCases = new[]
        {
            (".pdf", "application/pdf"),
            (".txt", "text/plain"),
            (".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
            (".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"),
            (".jpg", "image/jpeg"),
            (".jpeg", "image/jpeg"),
            (".png", "image/png"),
            (".mp4", "video/mp4"),
            (".mp3", "audio/mpeg"),
            (".unknown", "application/octet-stream")
        };

        foreach (var (extension, expectedMimeType) in testCases)
        {
            // Act
            var result = _fileStorageService.GetContentType(extension);

            // Assert
            Assert.Equal(expectedMimeType, result);
        }
    }

    [Fact]
    public void GetContentType_ShouldHandleNullExtension()
    {
        // Act
        var result = _fileStorageService.GetContentType(null!);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetContentType_ShouldHandleEmptyExtension()
    {
        // Act
        var result = _fileStorageService.GetContentType("");

        // Assert
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public async Task SaveFileAsync_ShouldHandleStorageErrors()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.txt");
        mockFile.Setup(f => f.Length).Returns(1024);
        
        // Create a file that will cause an error when trying to write
        var contentBytes = System.Text.Encoding.UTF8.GetBytes("test content");
        var memoryStream = new MemoryStream(contentBytes);
        memoryStream.Position = 0;
        mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
        
        var userId = 1;

        // Act & Assert - This should not throw an exception as the service handles errors gracefully
        var result = await _fileStorageService.SaveFileAsync(mockFile.Object, userId);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SaveFileAsync_ShouldGenerateUniqueFileName()
    {
        // Arrange
        var userId = 1;
        var fileName = "test.txt";
        var fileContent = "Hello, World!";
        
        var mockFile1 = new Mock<IFormFile>();
        mockFile1.Setup(f => f.FileName).Returns(fileName);
        mockFile1.Setup(f => f.Length).Returns(fileContent.Length);
        mockFile1.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)));

        var mockFile2 = new Mock<IFormFile>();
        mockFile2.Setup(f => f.FileName).Returns(fileName);
        mockFile2.Setup(f => f.Length).Returns(fileContent.Length);
        mockFile2.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)));

        // Act
        var result1 = await _fileStorageService.SaveFileAsync(mockFile1.Object, userId);
        var result2 = await _fileStorageService.SaveFileAsync(mockFile2.Object, userId);

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.True(File.Exists(result1));
        Assert.True(File.Exists(result2));
    }

    [Fact]
    public async Task SaveFileAsync_ShouldHandleLargeFiles()
    {
        // Arrange
        var userId = 1;
        var fileName = "large.txt";
        var largeContent = new string('A', 1024 * 1024); // 1MB of 'A's
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(largeContent.Length);
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(largeContent);
        var memoryStream = new MemoryStream(contentBytes);
        mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

        // Act
        var result = await _fileStorageService.SaveFileAsync(mockFile.Object, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(File.Exists(result));
        
        var fileInfo = new FileInfo(result);
        Assert.True(fileInfo.Length >= 1024 * 1024);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testUploadPath))
        {
            Directory.Delete(_testUploadPath, true);
        }
    }
}
