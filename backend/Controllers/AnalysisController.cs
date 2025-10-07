using Microsoft.AspNetCore.Mvc;
using StudentStudyAI.Models;
using StudentStudyAI.Services;

namespace StudentStudyAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        [HttpPost("analyze")]
        public IActionResult Analyze([FromBody] AnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest(new { error = "Content is required", message = "Please provide content to analyze" });
                }

                if (string.IsNullOrEmpty(request.FileType))
                {
                    return BadRequest(new { error = "File type is required", message = "Please specify the file type" });
                }

                // For now, return mock data based on content length
                // This simulates different analysis results
                var analysis = GetMockAnalysis(request.Content);

                return Ok(analysis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Analysis failed", message = ex.Message });
            }
        }

        [HttpGet("status/{analysisId}")]
        public IActionResult GetAnalysisStatus(string analysisId)
        {
            // Mock status check
            return Ok(new { 
                analysisId = analysisId, 
                status = "completed", 
                progress = 100 
            });
        }

        private AnalysisResult GetMockAnalysis(string content)
        {
            // Simple logic to return different mock data based on content
            var contentLength = content.Length;
            
            if (contentLength > 1000)
                return MockData.GetGoodAnalysis();
            else if (contentLength > 500)
                return MockData.GetAverageAnalysis();
            else
                return MockData.GetPoorAnalysis();
        }
    }

    public class AnalysisRequest
    {
        public string FileType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string StudentLevel { get; set; } = "intermediate";
    }
}
