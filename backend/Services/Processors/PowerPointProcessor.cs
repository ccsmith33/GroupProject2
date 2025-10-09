using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml;
using System.Text;

namespace StudentStudyAI.Services.Processors
{
    public class PowerPointProcessor : IPowerPointProcessor
    {
        private readonly ILogger<PowerPointProcessor> _logger;

        public PowerPointProcessor(ILogger<PowerPointProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            try
            {
                using var presentation = PresentationDocument.Open(filePath, false);
                var presentationPart = presentation.PresentationPart;
                
                if (presentationPart?.Presentation == null)
                {
                    return string.Empty;
                }

                var text = new StringBuilder();
                var slideParts = presentationPart.SlideParts;

                foreach (var slidePart in slideParts)
                {
                    var slide = slidePart.Slide;
                    ExtractTextFromSlide(slide, text);
                }

                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PowerPoint file: {FilePath}", filePath);
                return string.Empty;
            }
        }

        public async Task<List<string>> ExtractImagesAsync(string filePath)
        {
            try
            {
                var images = new List<string>();
                using var presentation = PresentationDocument.Open(filePath, false);
                var presentationPart = presentation.PresentationPart;
                
                if (presentationPart?.Presentation == null)
                {
                    return images;
                }

                var slideParts = presentationPart.SlideParts;

                foreach (var slidePart in slideParts)
                {
                    var slide = slidePart.Slide;
                    ExtractImagesFromSlide(slide, images);
                }

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting images from PowerPoint file: {FilePath}", filePath);
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, object>> ExtractMetadataAsync(string filePath)
        {
            try
            {
                var metadata = new Dictionary<string, object>();
                using var presentation = PresentationDocument.Open(filePath, false);
                var presentationPart = presentation.PresentationPart;
                
                if (presentationPart?.Presentation == null)
                {
                    return metadata;
                }

                // Extract basic metadata
                metadata["SlideCount"] = presentationPart.SlideParts.Count();
                metadata["FileType"] = "PowerPoint";
                metadata["ExtractedAt"] = DateTime.UtcNow;

                // Extract slide titles
                var slideTitles = new List<string>();
                foreach (var slidePart in presentationPart.SlideParts)
                {
                    var slide = slidePart.Slide;
                    var title = ExtractSlideTitle(slide);
                    if (!string.IsNullOrEmpty(title))
                    {
                        slideTitles.Add(title);
                    }
                }
                metadata["SlideTitles"] = slideTitles;

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting metadata from PowerPoint file: {FilePath}", filePath);
                return new Dictionary<string, object>();
            }
        }

        private void ExtractTextFromSlide(Slide slide, StringBuilder text)
        {
            if (slide == null) return;

            // Extract text from all text elements in the slide
            var textElements = slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>();
            foreach (var textElement in textElements)
            {
                if (!string.IsNullOrWhiteSpace(textElement.Text))
                {
                    text.AppendLine(textElement.Text);
                }
            }
        }

        private void ExtractImagesFromSlide(Slide slide, List<string> images)
        {
            if (slide == null) return;

            // Extract image references from the slide
            var imageParts = slide.Descendants<Blip>();
            foreach (var imagePart in imageParts)
            {
                if (imagePart.Embed != null)
                {
                    images.Add($"Image: {imagePart.Embed}");
                }
            }
        }

        private string ExtractSlideTitle(Slide slide)
        {
            if (slide == null) return string.Empty;

            // Try to find the first text element which is usually the title
            var firstText = slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>().FirstOrDefault();
            return firstText?.Text ?? string.Empty;
        }
    }
}
