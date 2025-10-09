using StudentStudyAI.Models;
using StudentStudyAI.Utils;
using System.Text.Json;

namespace StudentStudyAI.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly OpenAIService _openAIService;
        private readonly ContextService _contextService;
        private readonly IDatabaseService _databaseService;
        private readonly ContentAnalysisService _contentAnalysisService;
        private readonly ILogger<AnalysisService> _logger;

        public AnalysisService(OpenAIService openAIService, ContextService contextService, IDatabaseService databaseService, ContentAnalysisService contentAnalysisService, ILogger<AnalysisService> logger)
        {
            _openAIService = openAIService;
            _contextService = contextService;
            _databaseService = databaseService;
            _contentAnalysisService = contentAnalysisService;
            _logger = logger;
        }

        public async Task<FileAnalysis> AnalyzeFileAsync(int fileId, int userId)
        {
            var file = await _databaseService.GetFileUploadAsync(fileId);
            if (file == null)
            {
                throw new ArgumentException("File not found");
            }

            // Get context for the file
            var context = await _contextService.GetContextForFileUploadAsync(userId, file);

            // Analyze the file content
            var analysisResult = await _openAIService.AnalyzeFileContentAsync(file, context.Subject, context.Topic);

            // Parse the AI response into FileAnalysis
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var parser = new AIResponseParser(loggerFactory.CreateLogger<AIResponseParser>());
            var fileAnalysis = parser.ParseFileAnalysis(analysisResult.Summary);
            fileAnalysis.FileId = fileId;

            // Store in database
            await _databaseService.CreateAnalysisResultAsync(new AnalysisResult
            {
                FileUploadId = fileId,
                UserId = userId,
                Subject = context.Subject,
                Topic = context.Topic,
                Feedback = analysisResult.Summary,
                CreatedAt = DateTime.UtcNow
            });

            return fileAnalysis;
        }

        public async Task<AnalysisResult> AnalyzeFileAsync(FileUpload file, int userId)
        {
            // Get context for the file
            var context = await _contextService.GetContextForFileUploadAsync(userId, file);

            // Analyze the file content
            var analysisResult = await _openAIService.AnalyzeFileContentAsync(file, context.Subject, context.Topic);

            // Create analysis result
            var result = new AnalysisResult
            {
                OverallScore = 85.0m, // Default score, could be extracted from AI response
                Feedback = JsonSerializer.Serialize(new { Summary = analysisResult.Summary, Strengths = new List<string>(), WeakAreas = new List<string>(), Recommendations = new List<string>() }),
                StudyPlan = JsonSerializer.Serialize(new { PriorityTopics = new List<string>(), SuggestedResources = new List<string>(), EstimatedTime = "1-2 hours", NextSteps = new List<string>() }),
                CreatedAt = DateTime.UtcNow
            };

            // Store in database
            result.UserId = userId;
            result.FileUploadId = file.Id;
            result.Subject = context.Subject;
            result.Topic = context.Topic;

            if (result.UserId == null)
            {
                throw new InvalidOperationException("UserId cannot be null when creating analysis result");
            }

            await _databaseService.CreateAnalysisResultAsync(result);

            return result;
        }

        public async Task<StudyGuide> GenerateStudyGuideAsync(string userPrompt, int userId)
        {
            try
            {
                _logger.LogInformation("Starting study guide generation for user {UserId} with prompt: '{Prompt}'", userId, userPrompt);

                // Get context for the user
                var context = await _contextService.GetContextForUserAsync(userId, userPrompt);
                _logger.LogInformation("Retrieved context - Files: {FileCount}, Subject: '{Subject}', Topic: '{Topic}'", 
                    context.Files.Count, context.Subject ?? "NULL", context.Topic ?? "NULL");

                // Generate study guide using AI
                var studyGuideContent = await _openAIService.GenerateStudyGuideAsync(
                    userPrompt, 
                    context.Files, 
                    context.StudyGuides, 
                    context.Subject, 
                    context.Topic
                );
                
                _logger.LogInformation("Generated study guide content length: {ContentLength}", studyGuideContent?.Length ?? 0);

                // Create study guide
                var extractedTitle = ExtractTitleFromContent(studyGuideContent ?? "");
                var title = !string.IsNullOrWhiteSpace(extractedTitle) ? extractedTitle : "Study Guide";
                
                // Ensure title is never null
                if (title == null)
                {
                    _logger.LogWarning("Title is null after extraction, setting default");
                    title = "Study Guide";
                }
                
                _logger.LogInformation("Final title: '{Title}'", title);
                
                var studyGuide = new StudyGuide
                {
                    UserId = userId,
                    Title = title,
                    Content = studyGuideContent,
                    Subject = context.Subject,
                    Topic = context.Topic,
                    SourceFileIds = JsonSerializer.Serialize(context.Files.Select(f => f.Id).ToList()),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Validate the study guide before database insert
                if (studyGuide.Title == null)
                {
                    _logger.LogError("StudyGuide object has null Title after creation - this should not happen!");
                    studyGuide.Title = "Study Guide";
                }

                _logger.LogInformation("About to save study guide to database - Title: '{Title}', UserId: {UserId}", 
                    studyGuide.Title, studyGuide.UserId);

                // Store in database
                var studyGuideId = await _databaseService.CreateStudyGuideAsync(studyGuide);
                studyGuide.Id = studyGuideId;
                
                _logger.LogInformation("Study guide created successfully with ID: {StudyGuideId}", studyGuideId);

                // Create conversation record
                var conversation = new Conversation
                {
                    UserId = userId,
                    Prompt = userPrompt,
                    Response = studyGuideContent,
                    ContextFileIds = JsonSerializer.Serialize(context.Files.Select(f => f.Id).ToList()),
                    ContextStudyGuideIds = JsonSerializer.Serialize(context.StudyGuides.Select(sg => sg.Id).ToList()),
                    Subject = context.Subject,
                    Topic = context.Topic,
                    CreatedAt = DateTime.UtcNow
                };

                await _databaseService.CreateConversationAsync(conversation);

                return studyGuide;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating study guide for user {UserId} with prompt: '{Prompt}'", userId, userPrompt);
                throw new InvalidOperationException($"Study guide generation failed: {ex.Message}", ex);
            }
        }

        public async Task<Quiz> GenerateQuizAsync(string userPrompt, int userId)
        {
            // Get context for the user
            var context = await _contextService.GetContextForUserAsync(userId, userPrompt);

            // Analyze content to determine optimal quiz settings
            var contentAnalysis = _contentAnalysisService.AnalyzeContent(context.Files);
            var questionCount = contentAnalysis.EstimatedQuestions;
            var knowledgeLevel = contentAnalysis.KnowledgeLevel;

            _logger.LogInformation("Generating quiz with {QuestionCount} questions for {KnowledgeLevel} level", 
                questionCount, knowledgeLevel);

            // Generate quiz using AI with dynamic parameters
            var quizContent = await _openAIService.GenerateQuizAsync(
                userPrompt, 
                context.Files, 
                context.StudyGuides, 
                context.Subject, 
                context.Topic,
                questionCount,
                knowledgeLevel
            );

            // Parse quiz content
            var quizData = ParseQuizContent(quizContent);

            // Create quiz
            var quiz = new Quiz
            {
                UserId = userId,
                Title = quizData.Title,
                Questions = JsonSerializer.Serialize(quizData.Questions),
                SourceFileIds = JsonSerializer.Serialize(context.Files.Select(f => f.Id).ToList()),
                SourceStudyGuideIds = JsonSerializer.Serialize(context.StudyGuides.Select(sg => sg.Id).ToList()),
                Subject = context.Subject,
                Topic = context.Topic,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Store in database
            var quizId = await _databaseService.CreateQuizAsync(quiz);
            quiz.Id = quizId;

            // Create conversation record
            var conversation = new Conversation
            {
                UserId = userId,
                Prompt = userPrompt,
                Response = quizContent,
                ContextFileIds = JsonSerializer.Serialize(context.Files.Select(f => f.Id).ToList()),
                ContextStudyGuideIds = JsonSerializer.Serialize(context.StudyGuides.Select(sg => sg.Id).ToList()),
                Subject = context.Subject,
                Topic = context.Topic,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.CreateConversationAsync(conversation);

            return quiz;
        }

        public async Task<string> GenerateConversationalResponseAsync(string userPrompt, int userId)
        {
            // Get context for the user
            var context = await _contextService.GetContextForUserAsync(userId, userPrompt);

            // Generate conversational response using AI
            var response = await _openAIService.GenerateConversationalResponseAsync(
                userPrompt, 
                context.Files, 
                context.StudyGuides, 
                context.Conversations, 
                context.Subject, 
                context.Topic
            );

            // Create conversation record
            var conversation = new Conversation
            {
                UserId = userId,
                Prompt = userPrompt,
                Response = response,
                ContextFileIds = JsonSerializer.Serialize(context.Files.Select(f => f.Id).ToList()),
                ContextStudyGuideIds = JsonSerializer.Serialize(context.StudyGuides.Select(sg => sg.Id).ToList()),
                Subject = context.Subject,
                Topic = context.Topic,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.CreateConversationAsync(conversation);

            return response;
        }

        private string? ExtractTitleFromContent(string content)
        {
            // Try to extract title from markdown heading
            var titleMatch = System.Text.RegularExpressions.Regex.Match(content, @"^#\s+(.+)$", System.Text.RegularExpressions.RegexOptions.Multiline);
            if (titleMatch.Success)
            {
                return titleMatch.Groups[1].Value.Trim();
            }

            // Try to extract from first line
            var firstLine = content.Split('\n').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(firstLine) && firstLine.Length < 100)
            {
                return firstLine;
            }

            return null;
        }

        private QuizData ParseQuizContent(string content)
        {
            try
            {
                var quizData = JsonSerializer.Deserialize<QuizData>(content);
                return quizData ?? new QuizData { Title = "Quiz", Questions = new List<QuizQuestion>() };
            }
            catch
            {
                // Fallback if JSON parsing fails
                return new QuizData
                {
                    Title = "Quiz",
                    Questions = new List<QuizQuestion>
                    {
                        new QuizQuestion
                        {
                            Id = 1,
                            Question = "Sample question based on your materials",
                            Options = new List<string> { "Option A", "Option B", "Option C", "Option D" },
                            CorrectAnswerIndex = 0,
                            Explanation = "This is a sample question."
                        }
                    }
                };
            }
        }
    }

    public class QuizData
    {
        public string Title { get; set; } = "";
        public List<QuizQuestion> Questions { get; set; } = new();
    }
}
