using System.Text.Json;
using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public class AIResponseParser
    {
        private readonly ILogger<AIResponseParser> _logger;

        public AIResponseParser(ILogger<AIResponseParser> logger)
        {
            _logger = logger;
        }

        public StudyGuide ParseStudyGuide(string aiResponse)
        {
            try
            {
                // Try to parse as JSON first
                if (aiResponse.TrimStart().StartsWith("{"))
                {
                    var jsonDoc = JsonDocument.Parse(aiResponse);
                    var root = jsonDoc.RootElement;

                    return new StudyGuide
                    {
                        Title = GetStringProperty(root, "title") ?? "Generated Study Guide",
                        Subject = GetStringProperty(root, "subject") ?? "",
                        Level = GetStringProperty(root, "level") ?? "",
                        Content = GetStringProperty(root, "content") ?? aiResponse,
                        KeyPoints = GetStringArrayProperty(root, "keyPoints") ?? new List<string>(),
                        Summary = GetStringProperty(root, "summary") ?? "",
                        CreatedAt = DateTime.UtcNow,
                        Status = "completed"
                    };
                }

                // Fallback to text parsing
                return ParseStudyGuideFromText(aiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing study guide from AI response");
                return new StudyGuide
                {
                    Title = "Generated Study Guide",
                    Content = aiResponse,
                    CreatedAt = DateTime.UtcNow,
                    Status = "completed"
                };
            }
        }

        public Quiz ParseQuiz(string aiResponse)
        {
            try
            {
                // Try to parse as JSON first
                if (aiResponse.TrimStart().StartsWith("{"))
                {
                    var jsonDoc = JsonDocument.Parse(aiResponse);
                    var root = jsonDoc.RootElement;

                    var questions = new List<QuizQuestion>();
                    if (root.TryGetProperty("questions", out var questionsElement) && questionsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var questionElement in questionsElement.EnumerateArray())
                        {
                            var question = new QuizQuestion
                            {
                                Question = GetStringProperty(questionElement, "question") ?? "",
                                Options = GetStringArrayProperty(questionElement, "options") ?? new List<string>(),
                                CorrectAnswer = GetStringProperty(questionElement, "correctAnswer") ?? "",
                                Explanation = GetStringProperty(questionElement, "explanation") ?? ""
                            };
                            questions.Add(question);
                        }
                    }

                    return new Quiz
                    {
                        Title = GetStringProperty(root, "title") ?? "Generated Quiz",
                        Subject = GetStringProperty(root, "subject") ?? "",
                        Level = GetStringProperty(root, "level") ?? "",
                        QuestionsList = questions,
                        CreatedAt = DateTime.UtcNow,
                        Status = "completed"
                    };
                }

                // Fallback to text parsing
                return ParseQuizFromText(aiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing quiz from AI response");
                return new Quiz
                {
                    Title = "Generated Quiz",
                    QuestionsList = new List<QuizQuestion>(),
                    CreatedAt = DateTime.UtcNow,
                    Status = "completed"
                };
            }
        }

        public FileAnalysis ParseFileAnalysis(string aiResponse)
        {
            try
            {
                // Try to parse as JSON first
                if (aiResponse.TrimStart().StartsWith("{"))
                {
                    var jsonDoc = JsonDocument.Parse(aiResponse);
                    var root = jsonDoc.RootElement;

                    return new FileAnalysis
                    {
                        FileId = 0, // Will be set by caller
                        AnalysisType = GetStringProperty(root, "analysisType") ?? "general",
                        Subject = GetStringProperty(root, "subject") ?? "",
                        Topic = GetStringProperty(root, "topic") ?? "",
                        KeyPoints = GetStringArrayProperty(root, "keyPoints") ?? new List<string>(),
                        Summary = GetStringProperty(root, "summary") ?? "",
                        Difficulty = GetStringProperty(root, "difficulty") ?? "medium",
                        Recommendations = GetStringArrayProperty(root, "recommendations") ?? new List<string>(),
                        CreatedAt = DateTime.UtcNow,
                        Status = "completed"
                    };
                }

                // Fallback to text parsing
                return ParseFileAnalysisFromText(aiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing file analysis from AI response");
                return new FileAnalysis
                {
                    AnalysisType = "general",
                    Summary = aiResponse,
                    CreatedAt = DateTime.UtcNow,
                    Status = "completed"
                };
            }
        }

        public List<string> ExtractKeyPoints(string content)
        {
            try
            {
                var keyPoints = new List<string>();
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("•") || trimmedLine.StartsWith("-") || trimmedLine.StartsWith("*") || 
                        trimmedLine.StartsWith("1.") || trimmedLine.StartsWith("2.") || trimmedLine.StartsWith("3.") ||
                        trimmedLine.StartsWith("Key Point") || trimmedLine.StartsWith("Important"))
                    {
                        keyPoints.Add(trimmedLine);
                    }
                }

                return keyPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting key points from content");
                return new List<string>();
            }
        }

        private StudyGuide ParseStudyGuideFromText(string text)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var title = "Generated Study Guide";
            var keyPoints = new List<string>();
            var content = text;

            // Try to extract title
            if (lines.Length > 0 && lines[0].Length < 100)
            {
                title = lines[0].Trim();
            }

            // Extract key points
            keyPoints = ExtractKeyPoints(text);

            return new StudyGuide
            {
                Title = title,
                Content = content,
                KeyPoints = keyPoints,
                CreatedAt = DateTime.UtcNow,
                Status = "completed"
            };
        }

        private Quiz ParseQuizFromText(string text)
        {
            var questions = new List<QuizQuestion>();
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            QuizQuestion? currentQuestion = null;
            var questionNumber = 1;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (trimmedLine.StartsWith($"{questionNumber}.") || trimmedLine.StartsWith("Q" + questionNumber))
                {
                    if (currentQuestion != null)
                    {
                        questions.Add(currentQuestion);
                    }
                    
                    currentQuestion = new QuizQuestion
                    {
                        Question = trimmedLine,
                        Options = new List<string>()
                    };
                    questionNumber++;
                }
                else if (currentQuestion != null && (trimmedLine.StartsWith("A)") || trimmedLine.StartsWith("B)") || 
                         trimmedLine.StartsWith("C)") || trimmedLine.StartsWith("D)") || trimmedLine.StartsWith("a)") || 
                         trimmedLine.StartsWith("b)") || trimmedLine.StartsWith("c)") || trimmedLine.StartsWith("d)")))
                {
                    currentQuestion.Options.Add(trimmedLine);
                }
            }

            if (currentQuestion != null)
            {
                questions.Add(currentQuestion);
            }

            return new Quiz
            {
                Title = "Generated Quiz",
                QuestionsList = questions,
                CreatedAt = DateTime.UtcNow,
                Status = "completed"
            };
        }

        private FileAnalysis ParseFileAnalysisFromText(string text)
        {
            var keyPoints = ExtractKeyPoints(text);
            
            return new FileAnalysis
            {
                AnalysisType = "general",
                Summary = text,
                KeyPoints = keyPoints,
                CreatedAt = DateTime.UtcNow,
                Status = "completed"
            };
        }

        private string? GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
            return null;
        }

        private List<string>? GetStringArrayProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
            {
                var list = new List<string>();
                foreach (var item in property.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        list.Add(item.GetString() ?? "");
                    }
                }
                return list;
            }
            return null;
        }
    }
}
