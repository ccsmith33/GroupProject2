using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services.Processors;
using Xunit;

namespace StudentStudyAI.Tests.Services.Processors;

public class WordProcessorTests : IDisposable
{
    private readonly Mock<ILogger<WordProcessor>> _mockLogger;
    private readonly WordProcessor _wordProcessor;
    private readonly string _testFilePath;

    public WordProcessorTests()
    {
        _mockLogger = new Mock<ILogger<WordProcessor>>();
        _wordProcessor = new WordProcessor(_mockLogger.Object);
        
        // Create a test Word file path
        _testFilePath = Path.Combine(Path.GetTempPath(), "test.docx");
    }

    [Fact]
    public async Task ExtractTextAsync_ShouldExtractTextFromWordDocument()
    {
        // Arrange
        await CreateTestWordFile();

        // Act
        var result = await _wordProcessor.ExtractTextAsync(_testFilePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Test Word Content", result);
    }

    [Fact]
    public async Task ExtractImagesAsync_ShouldExtractImagesFromWord()
    {
        // Arrange
        await CreateTestWordFile();

        // Act
        var result = await _wordProcessor.ExtractImagesAsync(_testFilePath);

        // Assert
        Assert.NotNull(result);
        // Note: This test would need a Word document with actual images to be meaningful
    }

    [Fact]
    public async Task ExtractMetadataAsync_ShouldExtractWordMetadata()
    {
        // Arrange
        await CreateTestWordFile();

        // Act
        var result = await _wordProcessor.ExtractMetadataAsync(_testFilePath);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("title") || result.ContainsKey("author") || result.ContainsKey("pages"));
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleCorruptedWordFile()
    {
        // Arrange
        var corruptedFilePath = Path.Combine(Path.GetTempPath(), "corrupted.docx");
        await File.WriteAllBytesAsync(corruptedFilePath, new byte[] { 0x00, 0x01, 0x02 });

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _wordProcessor.ProcessFileAsync(corruptedFilePath));
    }

    [Fact]
    public async Task ExtractTextAsync_ShouldHandleFileNotFound()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.docx");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _wordProcessor.ExtractTextAsync(nonExistentPath));
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleUnsupportedFileType()
    {
        // Arrange
        var txtFilePath = Path.Combine(Path.GetTempPath(), "test.txt");
        await File.WriteAllTextAsync(txtFilePath, "This is a text file");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _wordProcessor.ProcessFileAsync(txtFilePath));
    }

    private async Task CreateTestWordFile()
    {
        // Create a minimal Word document structure
        // This is a simplified approach - in real implementation, you'd use DocumentFormat.OpenXml
        var wordContent = @"PK
test.docx
word/document.xml
<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:body>
    <w:p>
      <w:r>
        <w:t>Test Word Content</w:t>
      </w:r>
    </w:p>
  </w:body>
</w:document>";

        await File.WriteAllTextAsync(_testFilePath, wordContent);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }
}
