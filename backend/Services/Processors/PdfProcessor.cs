using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using StudentStudyAI.Models;

namespace StudentStudyAI.Services.Processors
{
    public class PdfProcessor : IPdfProcessor
    {
        private readonly ILogger<PdfProcessor> _logger;

        public PdfProcessor(ILogger<PdfProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            try
            {
                using var reader = new PdfReader(filePath);
                var text = new System.Text.StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.AppendLine(PdfTextExtractor.GetTextFromPage(reader, i));
                }

                _logger.LogInformation("Extracted text from PDF: {FilePath}, Pages: {PageCount}", filePath, reader.NumberOfPages);
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<List<string>> ExtractImagesAsync(string filePath)
        {
            try
            {
                var imagePaths = new List<string>();
                using var reader = new PdfReader(filePath);

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    var page = reader.GetPageN(i);
                    var resources = page.GetAsDict(PdfName.RESOURCES);
                    var xObjects = resources?.GetAsDict(PdfName.XOBJECT);

                    if (xObjects != null)
                    {
                        foreach (var key in xObjects.Keys)
                        {
                            var obj = xObjects.Get(key);
                            if (obj.IsIndirect())
                            {
                                var pdfStream = (PdfStream)PdfReader.GetPdfObject(obj);
                                var subtype = pdfStream.GetAsName(PdfName.SUBTYPE);

                                if (PdfName.IMAGE.Equals(subtype))
                                {
                                    // Extract image data
                                    var imageData = PdfReader.GetStreamBytesRaw((PRStream)pdfStream);
                                    var imagePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath)!, $"image_{i}_{key}.jpg");
                                    await File.WriteAllBytesAsync(imagePath, imageData);
                                    imagePaths.Add(imagePath);
                                }
                            }
                        }
                    }
                }

                _logger.LogInformation("Extracted {ImageCount} images from PDF: {FilePath}", imagePaths.Count, filePath);
                return imagePaths;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting images from PDF: {FilePath}", filePath);
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, object>> ExtractMetadataAsync(string filePath)
        {
            try
            {
                using var reader = new PdfReader(filePath);
                var metadata = new Dictionary<string, object>
                {
                    ["PageCount"] = reader.NumberOfPages,
                    ["Title"] = reader.Info.ContainsKey("Title") ? reader.Info["Title"] : "",
                    ["Author"] = reader.Info.ContainsKey("Author") ? reader.Info["Author"] : "",
                    ["Subject"] = reader.Info.ContainsKey("Subject") ? reader.Info["Subject"] : "",
                    ["Keywords"] = reader.Info.ContainsKey("Keywords") ? reader.Info["Keywords"] : "",
                    ["Creator"] = reader.Info.ContainsKey("Creator") ? reader.Info["Creator"] : "",
                    ["Producer"] = reader.Info.ContainsKey("Producer") ? reader.Info["Producer"] : "",
                    ["CreationDate"] = reader.Info.ContainsKey("CreationDate") ? reader.Info["CreationDate"] : "",
                    ["ModDate"] = reader.Info.ContainsKey("ModDate") ? reader.Info["ModDate"] : ""
                };

                _logger.LogInformation("Extracted metadata from PDF: {FilePath}", filePath);
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting metadata from PDF: {FilePath}", filePath);
                return new Dictionary<string, object>();
            }
        }
    }
}
