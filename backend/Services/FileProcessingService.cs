using StudentStudyAI.Models;
using StudentStudyAI.Utils;
using StudentStudyAI.Services.Processors;
using Microsoft.AspNetCore.Http;

namespace StudentStudyAI.Services
{
    public class FileProcessingService : IFileProcessingService
    {
    private readonly IPdfProcessor _pdfProcessor;
    private readonly IWordProcessor _wordProcessor;
    private readonly IPowerPointProcessor _powerPointProcessor;
    private readonly IImageProcessor _imageProcessor;
    private readonly IMediaProcessor _mediaProcessor;
        private readonly ILogger<FileProcessingService> _logger;

        public FileProcessingService(
            IPdfProcessor pdfProcessor,
            IWordProcessor wordProcessor,
            IPowerPointProcessor powerPointProcessor,
            IImageProcessor imageProcessor,
            IMediaProcessor mediaProcessor,
            ILogger<FileProcessingService> logger)
        {
            _pdfProcessor = pdfProcessor;
            _wordProcessor = wordProcessor;
            _powerPointProcessor = powerPointProcessor;
            _imageProcessor = imageProcessor;
            _mediaProcessor = mediaProcessor;
            _logger = logger;
        }

        public async Task<ProcessedFile> ProcessFileAsync(FileUpload file)
        {
            try
            {
                _logger.LogInformation("Starting file processing for: {FileName}", file.FileName);

                var processedFile = new ProcessedFile
                {
                    FileId = file.Id,
                    FileName = file.FileName,
                    FileType = file.FileType,
                    ProcessedAt = DateTime.UtcNow,
                    Status = "processing"
                };

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string extractedText = string.Empty;
                List<string> extractedImages = new();
                Dictionary<string, object> metadata = new();

                switch (fileExtension)
                {
                    case ".pdf":
                        extractedText = await _pdfProcessor.ExtractTextAsync(file.FilePath);
                        extractedImages = await _pdfProcessor.ExtractImagesAsync(file.FilePath);
                        metadata = await _pdfProcessor.ExtractMetadataAsync(file.FilePath);
                        break;

                    case ".docx":
                    case ".doc":
                        extractedText = await _wordProcessor.ExtractTextAsync(file.FilePath);
                        extractedImages = await _wordProcessor.ExtractImagesAsync(file.FilePath);
                        metadata = await _wordProcessor.ExtractMetadataAsync(file.FilePath);
                        break;

                    case ".pptx":
                    case ".ppt":
                        extractedText = await _powerPointProcessor.ExtractTextAsync(file.FilePath);
                        extractedImages = await _powerPointProcessor.ExtractImagesAsync(file.FilePath);
                        metadata = await _powerPointProcessor.ExtractMetadataAsync(file.FilePath);
                        break;

                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                    case ".bmp":
                        extractedText = await _imageProcessor.ExtractTextAsync(file.FilePath);
                        extractedImages = await _imageProcessor.ExtractTextRegionsAsync(file.FilePath);
                        metadata = await _imageProcessor.AnalyzeImageAsync(file.FilePath);
                        break;

                    case ".txt":
                        extractedText = await File.ReadAllTextAsync(file.FilePath);
                        break;

                    case ".mp3":
                    case ".wav":
                    case ".flac":
                    case ".aac":
                    case ".ogg":
                    case ".wma":
                    case ".m4a":
                        extractedText = await _mediaProcessor.TranscribeAudioAsync(file.FilePath);
                        metadata = await _mediaProcessor.ExtractMetadataAsync(file.FilePath);
                        break;

                    case ".mp4":
                    case ".avi":
                    case ".mov":
                    case ".wmv":
                    case ".flv":
                    case ".webm":
                    case ".mkv":
                    case ".m4v":
                        extractedText = await _mediaProcessor.TranscribeAudioAsync(file.FilePath);
                        extractedImages = await _mediaProcessor.ExtractFramesAsync(file.FilePath);
                        metadata = await _mediaProcessor.ExtractMetadataAsync(file.FilePath);
                        break;

                    default:
                        _logger.LogWarning("Unsupported file type: {FileType}", fileExtension);
                        processedFile.Status = "unsupported";
                        return processedFile;
                }

                processedFile.ExtractedText = extractedText;
                processedFile.ExtractedImages = extractedImages;
                processedFile.Metadata = metadata;
                processedFile.Status = "completed";

                _logger.LogInformation("File processing completed for: {FileName}, Text length: {TextLength}", 
                    file.FileName, extractedText.Length);

                return processedFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FileName}", file.FileName);
                return new ProcessedFile
                {
                    FileId = file.Id,
                    FileName = file.FileName,
                    FileType = file.FileType,
                    ProcessedAt = DateTime.UtcNow,
                    Status = "error",
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> ValidateFileAsync(IFormFile file)
        {
            try
            {
                // Check file size (10MB limit)
                if (file.Length > 10 * 1024 * 1024)
                {
                    _logger.LogWarning("File too large: {FileName}, Size: {Size}", file.FileName, file.Length);
                    return false;
                }

                // Check file extension
                var allowedTypes = new[] { ".pdf", ".txt", ".docx", ".doc", ".pptx", ".ppt", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv", ".m4v" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedTypes.Contains(fileExtension))
                {
                    _logger.LogWarning("Unsupported file type: {FileName}, Type: {FileType}", file.FileName, fileExtension);
                    return false;
                }

                // Check for malicious file names
                if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
                {
                    _logger.LogWarning("Potentially malicious file name: {FileName}", file.FileName);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file: {FileName}", file.FileName);
                return false;
            }
        }
    }

    public class ProcessedFile
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string ExtractedText { get; set; } = string.Empty;
        public List<string> ExtractedImages { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
