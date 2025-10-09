using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.Text;

namespace StudentStudyAI.Services.Processors
{
    public class WordProcessor : IWordProcessor
    {
        private readonly ILogger<WordProcessor> _logger;

        public WordProcessor(ILogger<WordProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            try
            {
                using var document = WordprocessingDocument.Open(filePath, false);
                var body = document.MainDocumentPart?.Document?.Body;
                
                if (body == null)
                {
                    return string.Empty;
                }

                var text = new StringBuilder();
                ExtractTextFromElement(body, text);

                _logger.LogInformation("Extracted text from Word document: {FilePath}", filePath);
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from Word document: {FilePath}", filePath);
                throw;
            }
        }

        private void ExtractTextFromElement(OpenXmlElement element, StringBuilder text)
        {
            foreach (var child in element.Elements())
            {
                if (child is Paragraph paragraph)
                {
                    ExtractTextFromParagraph(paragraph, text);
                    text.AppendLine();
                }
                else if (child is Table table)
                {
                    ExtractTextFromTable(table, text);
                    text.AppendLine();
                }
                else
                {
                    ExtractTextFromElement(child, text);
                }
            }
        }

        private void ExtractTextFromParagraph(Paragraph paragraph, StringBuilder text)
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                foreach (var textElement in run.Elements<Text>())
                {
                    text.Append(textElement.Text);
                }
            }
        }

        private void ExtractTextFromTable(Table table, StringBuilder text)
        {
            foreach (var row in table.Elements<TableRow>())
            {
                foreach (var cell in row.Elements<TableCell>())
                {
                    ExtractTextFromElement(cell, text);
                    text.Append("\t");
                }
                text.AppendLine();
            }
        }

        public async Task<List<string>> ExtractImagesAsync(string filePath)
        {
            try
            {
                var imagePaths = new List<string>();
                using var document = WordprocessingDocument.Open(filePath, false);
                
                var imageParts = document.MainDocumentPart?.ImageParts;
                if (imageParts != null)
                {
                    var imageDir = Path.Combine(Path.GetDirectoryName(filePath)!, "images");
                    if (!Directory.Exists(imageDir))
                    {
                        Directory.CreateDirectory(imageDir);
                    }

                    int imageIndex = 0;
                    foreach (var imagePart in imageParts)
                    {
                        var imagePath = Path.Combine(imageDir, $"image_{imageIndex}.{GetImageExtension(imagePart.ContentType)}");
                        using var stream = imagePart.GetStream();
                        using var fileStream = new FileStream(imagePath, FileMode.Create);
                        await stream.CopyToAsync(fileStream);
                        imagePaths.Add(imagePath);
                        imageIndex++;
                    }
                }

                _logger.LogInformation("Extracted {ImageCount} images from Word document: {FilePath}", imagePaths.Count, filePath);
                return imagePaths;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting images from Word document: {FilePath}", filePath);
                return new List<string>();
            }
        }

        private string GetImageExtension(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => "jpg",
                "image/png" => "png",
                "image/gif" => "gif",
                "image/bmp" => "bmp",
                _ => "jpg"
            };
        }

        public async Task<Dictionary<string, object>> ExtractMetadataAsync(string filePath)
        {
            try
            {
                using var document = WordprocessingDocument.Open(filePath, false);
                var coreProperties = document.PackageProperties;

                var metadata = new Dictionary<string, object>
                {
                    ["Title"] = coreProperties?.Title ?? "",
                    ["Subject"] = coreProperties?.Subject ?? "",
                    ["Creator"] = coreProperties?.Creator ?? "",
                    ["Keywords"] = coreProperties?.Keywords ?? "",
                    ["Description"] = coreProperties?.Description ?? "",
                    ["LastModifiedBy"] = coreProperties?.LastModifiedBy ?? "",
                    ["Created"] = coreProperties?.Created?.ToString() ?? "",
                    ["Modified"] = coreProperties?.Modified?.ToString() ?? "",
                    ["Category"] = coreProperties?.Category ?? "",
                    ["Version"] = coreProperties?.Version ?? ""
                };

                _logger.LogInformation("Extracted metadata from Word document: {FilePath}", filePath);
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting metadata from Word document: {FilePath}", filePath);
                return new Dictionary<string, object>();
            }
        }
    }
}
