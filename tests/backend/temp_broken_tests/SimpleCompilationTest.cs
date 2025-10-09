using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using Xunit;

namespace StudentStudyAI.Tests.Services;

public class SimpleCompilationTest
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_password"
        };

        // Act & Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public void DatabaseService_ShouldBeCreatable()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns("Server=localhost;Database=test;Uid=test;Pwd=test;");

        // Act
        var databaseService = new DatabaseService(mockConfig.Object);

        // Assert
        Assert.NotNull(databaseService);
    }

    [Fact]
    public void JwtService_ShouldBeCreatable()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        jwtSection.Setup(x => x["Issuer"]).Returns("StudentStudyAI");
        jwtSection.Setup(x => x["Audience"]).Returns("StudentStudyAI");
        jwtSection.Setup(x => x["ExpiryMinutes"]).Returns("60");

        mockConfig.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

        var mockLogger = new Mock<ILogger<JwtService>>();

        // Act
        var jwtService = new JwtService(mockConfig.Object, mockLogger.Object);

        // Assert
        Assert.NotNull(jwtService);
    }

    [Fact]
    public void FileStorageService_ShouldBeCreatable()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["FileStorage:Path"]).Returns("test-uploads");

        var mockLogger = new Mock<ILogger<FileStorageService>>();

        // Act
        var fileStorageService = new FileStorageService(mockLogger.Object, mockConfig.Object);

        // Assert
        Assert.NotNull(fileStorageService);
    }

    [Fact]
    public void StudyGuide_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var studyGuide = new StudyGuide
        {
            Id = 1,
            UserId = 1,
            Title = "Test Study Guide",
            Content = "Test content",
            Subject = "Mathematics",
            Topic = "Algebra",
            Level = "Intermediate",
            KeyPoints = new List<string> { "Point 1", "Point 2" },
            Summary = "Test summary"
        };

        // Assert
        Assert.Equal(1, studyGuide.Id);
        Assert.Equal("Test Study Guide", studyGuide.Title);
        Assert.Equal("Test content", studyGuide.Content);
        Assert.Equal("Mathematics", studyGuide.Subject);
        Assert.Equal("Algebra", studyGuide.Topic);
        Assert.Equal("Intermediate", studyGuide.Level);
        Assert.Equal(2, studyGuide.KeyPoints.Count);
        Assert.Equal("Test summary", studyGuide.Summary);
    }

    [Fact]
    public void FileUpload_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var fileUpload = new FileUpload
        {
            Id = 1,
            UserId = 1,
            FileName = "test.pdf",
            FilePath = "/uploads/test.pdf",
            FileType = ".pdf",
            FileSize = 1024,
            Subject = "Mathematics",
            StudentLevel = "Intermediate",
            UploadedAt = DateTime.UtcNow,
            Status = "uploaded"
        };

        // Assert
        Assert.Equal(1, fileUpload.Id);
        Assert.Equal(1, fileUpload.UserId);
        Assert.Equal("test.pdf", fileUpload.FileName);
        Assert.Equal("/uploads/test.pdf", fileUpload.FilePath);
        Assert.Equal(".pdf", fileUpload.FileType);
        Assert.Equal(1024, fileUpload.FileSize);
        Assert.Equal("Mathematics", fileUpload.Subject);
        Assert.Equal("Intermediate", fileUpload.StudentLevel);
        Assert.Equal("uploaded", fileUpload.Status);
    }
}
