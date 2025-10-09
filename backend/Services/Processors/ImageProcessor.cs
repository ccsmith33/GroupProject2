using Tesseract;
using System.Drawing;
using System.Drawing.Imaging;

namespace StudentStudyAI.Services.Processors
{
    public class ImageProcessor : IImageProcessor
    {
        private readonly ILogger<ImageProcessor> _logger;
        private readonly string _tesseractDataPath;

        public ImageProcessor(ILogger<ImageProcessor> logger, IConfiguration configuration)
        {
            _logger = logger;
            _tesseractDataPath = configuration["Tesseract:DataPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            try
            {
                using var engine = new TesseractEngine(_tesseractDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                using var page = engine.Process(img);

                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                _logger.LogInformation("Extracted text from image: {FilePath}, Confidence: {Confidence}", filePath, confidence);
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image: {FilePath}", filePath);
                return string.Empty;
            }
        }

        public async Task<List<string>> ExtractTextRegionsAsync(string filePath)
        {
            try
            {
                var textRegions = new List<string>();
                using var engine = new TesseractEngine(_tesseractDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                using var page = engine.Process(img);

                using var iter = page.GetIterator();
                iter.Begin();

                do
                {
                    var text = iter.GetText(PageIteratorLevel.Word);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        textRegions.Add(text);
                    }
                } while (iter.Next(PageIteratorLevel.Word));

                _logger.LogInformation("Extracted {RegionCount} text regions from image: {FilePath}", textRegions.Count, filePath);
                return textRegions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text regions from image: {FilePath}", filePath);
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, object>> AnalyzeImageAsync(string filePath)
        {
            try
            {
                using var image = Image.FromFile(filePath);
                var metadata = new Dictionary<string, object>
                {
                    ["Width"] = image.Width,
                    ["Height"] = image.Height,
                    ["PixelFormat"] = image.PixelFormat.ToString(),
                    ["HorizontalResolution"] = image.HorizontalResolution,
                    ["VerticalResolution"] = image.VerticalResolution,
                    ["Size"] = new FileInfo(filePath).Length
                };

                // Extract EXIF data if available
                if (image.PropertyIdList != null)
                {
                    var exifData = new Dictionary<string, object>();
                    foreach (int propId in image.PropertyIdList)
                    {
                        var prop = image.GetPropertyItem(propId);
                        if (prop?.Value != null)
                        {
                            exifData[propId.ToString()] = prop.Value;
                        }
                    }
                    metadata["ExifData"] = exifData;
                }

                _logger.LogInformation("Analyzed image: {FilePath}, Size: {Width}x{Height}", filePath, image.Width, image.Height);
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image: {FilePath}", filePath);
                return new Dictionary<string, object>();
            }
        }
    }
}
