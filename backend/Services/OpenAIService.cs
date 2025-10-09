using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly int _maxTokens;
        private readonly double _temperature;

        public OpenAIService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["OpenAI:ApiKey"] ?? "mock-key";
            _baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
            _model = _configuration["OpenAI:Model"] ?? "gpt-5-nano";
            _maxTokens = int.Parse(_configuration["OpenAI:MaxTokens"] ?? "2000");
            _temperature = double.Parse(_configuration["OpenAI:Temperature"] ?? "0.7");
        }

        public async Task<string> GenerateStudyGuideAsync(string userPrompt, List<FileUpload> contextFiles, List<StudyGuide> contextStudyGuides, string? subject = null, string? topic = null)
        {
            var context = BuildContextString(contextFiles, contextStudyGuides, subject, topic);
            var systemPrompt = BuildStudyGuideSystemPrompt(subject, topic);
            
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"{context}\n\nUser Request: {userPrompt}" }
            };

            return await CallOpenAIAsync(messages);
        }

        public async Task<string> GenerateQuizAsync(string userPrompt, List<FileUpload> contextFiles, List<StudyGuide> contextStudyGuides, string? subject = null, string? topic = null, int? questionCount = null, KnowledgeLevel? knowledgeLevel = null)
        {
            var context = BuildContextString(contextFiles, contextStudyGuides, subject, topic);
            var systemPrompt = BuildDynamicQuizSystemPrompt(subject, topic, questionCount, knowledgeLevel);
            
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"{context}\n\nUser Request: {userPrompt}" }
            };

            return await CallOpenAIAsync(messages);
        }


        public async Task<string> GenerateConversationalResponseAsync(string userPrompt, List<FileUpload> contextFiles, List<StudyGuide> contextStudyGuides, List<Conversation> recentConversations, string? subject = null, string? topic = null)
        {
            var context = BuildContextString(contextFiles, contextStudyGuides, subject, topic);
            var conversationHistory = BuildConversationHistory(recentConversations);
            var systemPrompt = BuildConversationalSystemPrompt(subject, topic);
            
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Add conversation history
            foreach (var conv in recentConversations.Take(5))
            {
                messages.Add(new { role = "user", content = conv.Prompt });
                messages.Add(new { role = "assistant", content = conv.Response });
            }

            // Add current context and prompt
            messages.Add(new { role = "user", content = $"{context}\n\n{conversationHistory}\n\nCurrent Request: {userPrompt}" });

            return await CallOpenAIAsync(messages);
        }

        private async Task<string> CallOpenAIAsync(List<object> messages)
        {
            // For development, return mock data if no API key
            if (_apiKey == "mock-key" || string.IsNullOrEmpty(_apiKey))
            {
                return GenerateMockResponse(messages);
            }

            var requestBody = new
            {
                model = _model,
                messages = messages,
                max_tokens = 2000,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            string responseContent = "";
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();

                responseContent = await response.Content.ReadAsStringAsync();
                
                // Log the response for debugging
                _logger.LogInformation("OpenAI Response: {Response}", responseContent);
                
                // Check if response starts with 'T' (likely a text response, not JSON)
                if (responseContent.TrimStart().StartsWith("T"))
                {
                    _logger.LogWarning("OpenAI returned text response instead of JSON, falling back to mock data");
                    return GenerateMockResponse(messages);
                }
                
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return responseObj.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "No response generated";
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization failed. Response content: {Content}", responseContent);
                // Fallback to mock data on JSON parsing failure
                return GenerateMockResponse(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI API call failed");
                // Fallback to mock data on API failure
                return GenerateMockResponse(messages);
            }
        }

        private string BuildContextString(List<FileUpload> files, List<StudyGuide> studyGuides, string? subject, string? topic)
        {
            var context = new StringBuilder();

            if (files.Any())
            {
                context.AppendLine("## Relevant Files:");
                foreach (var file in files.Take(5))
                {
                    context.AppendLine($"### {file.FileName} ({file.FileType})");
                    if (!string.IsNullOrEmpty(file.Subject))
                        context.AppendLine($"Subject: {file.Subject}");
                    if (!string.IsNullOrEmpty(file.Topic))
                        context.AppendLine($"Topic: {file.Topic}");
                    
                    var content = file.ProcessedContent ?? file.Content;
                    if (!string.IsNullOrEmpty(content))
                    {
                        context.AppendLine($"Content: {content.Substring(0, Math.Min(content.Length, 1000))}...");
                    }
                    context.AppendLine();
                }
            }

            if (studyGuides.Any())
            {
                context.AppendLine("## Previous Study Guides:");
                foreach (var guide in studyGuides.Take(3))
                {
                    context.AppendLine($"### {guide.Title}");
                    if (!string.IsNullOrEmpty(guide.Subject))
                        context.AppendLine($"Subject: {guide.Subject}");
                    context.AppendLine($"Content: {guide.Content.Substring(0, Math.Min(guide.Content.Length, 500))}...");
                    context.AppendLine();
                }
            }

            return context.ToString();
        }

        private string BuildConversationHistory(List<Conversation> conversations)
        {
            if (!conversations.Any()) return "";

            var history = new StringBuilder();
            history.AppendLine("## Recent Conversation History:");
            
            foreach (var conv in conversations.Take(3))
            {
                history.AppendLine($"**User:** {conv.Prompt}");
                history.AppendLine($"**Assistant:** {conv.Response.Substring(0, Math.Min(conv.Response.Length, 200))}...");
                history.AppendLine();
            }

            return history.ToString();
        }

        private string BuildStudyGuideSystemPrompt(string? subject, string? topic)
        {
            return $@"You are an expert study guide generator and educational content creator. Your task is to create comprehensive, EXPANDED study guides that go BEYOND the provided materials to create deeper understanding.

Guidelines:
- EXPAND on the provided materials with additional context, examples, and connections
- Add real-world applications and practical examples not in the source material
- Include related concepts, historical context, and broader implications
- Create connections between different topics and subjects
- Provide multiple perspectives and approaches to understanding
- Include critical thinking questions and analysis prompts
- Add study strategies, memory techniques, and learning tips
- Incorporate current research, trends, or developments in the field
- Create analogies, metaphors, and visual descriptions to aid understanding
- Include practice problems, case studies, or scenarios for application
- Focus on the subject: {(subject ?? "general academic content")}
- Specific topic: {(topic ?? "as requested by the user")}
- Make content suitable for both children and college-level students
- Encourage deeper thinking and independent learning

EXPANSION REQUIREMENTS:
- Don't just summarize the provided content - ADD VALUE
- Include information that helps students understand WHY concepts matter
- Connect abstract concepts to concrete, relatable examples
- Provide multiple ways to approach and understand each topic
- Include common misconceptions and how to avoid them
- Add resources for further learning and exploration

Format your response as a comprehensive study guide with clear headings, sections, and expanded content.";
        }

        private string BuildQuizSystemPrompt(string? subject, string? topic)
        {
            return $@"You are an expert quiz generator and educational assessment creator. Create comprehensive, EXPANDED quizzes that test deep understanding and application, not just memorization.

Guidelines:
- Generate 8-12 high-quality multiple-choice questions that go BEYOND the provided materials
- Include 4 options (A, B, C, D) for each question with plausible distractors
- Create questions that test APPLICATION, ANALYSIS, and SYNTHESIS, not just recall
- Include scenario-based questions and real-world applications
- Test understanding of WHY concepts work, not just WHAT they are
- Include questions that connect different concepts from the materials
- Add questions about implications, consequences, and broader context
- Test critical thinking and problem-solving skills
- Include questions about common misconceptions and errors
- Vary question difficulty: 30% basic, 50% intermediate, 20% advanced
- Focus on the subject: {(subject ?? "general academic content")}
- Specific topic: {(topic ?? "as requested by the user")}
- Make questions suitable for both children and college-level students

EXPANSION REQUIREMENTS:
- Don't just ask about facts from the materials - test UNDERSTANDING
- Include questions that require students to apply concepts to new situations
- Add questions about connections between different topics
- Include questions about practical applications and real-world relevance
- Test ability to identify patterns, make predictions, and draw conclusions
- Include questions about cause-and-effect relationships
- Add questions that require synthesis of multiple concepts

QUESTION TYPES TO INCLUDE:
- Application scenarios: ""If X happens, what would be the result?""
- Analysis questions: ""What is the relationship between X and Y?""
- Synthesis questions: ""How would you combine concepts A and B to solve problem C?""
- Evaluation questions: ""Which approach would be most effective and why?""
- Prediction questions: ""Based on the principles, what would happen if...""

Format your response as JSON with this structure:
{{
  ""title"": ""Comprehensive Quiz Title"",
  ""questions"": [
    {{
      ""id"": 1,
      ""question"": ""Question text that tests deep understanding"",
      ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
      ""correctAnswerIndex"": 0,
      ""explanation"": ""Detailed explanation of why this answer is correct, including broader context and connections""
    }}
  ]
}}";
        }

        private string BuildAnalysisSystemPrompt(string? subject, string? topic)
        {
            return $@"You are an expert content analyzer for educational materials. Analyze the provided content and extract key information.

Guidelines:
- Identify the main subject and topic
- Extract key concepts and important points
- Note any questions, problems, or exercises
- Identify the difficulty level (elementary, middle school, high school, college)
- Suggest relevant study topics
- Extract any mathematical formulas, definitions, or important facts
- Note the structure and organization of the content
- Focus on the subject: {(subject ?? "general academic content")}
- Specific topic: {(topic ?? "as identified in the content")}

Format your response as a structured analysis with clear sections.";
        }

        private string BuildConversationalSystemPrompt(string? subject, string? topic)
        {
            return $@"You are an expert AI study assistant and educational mentor. Your role is to EXPAND students' understanding beyond their materials and help them develop deeper learning.

Guidelines:
- EXPAND on the provided materials with additional insights, connections, and context
- Don't just answer questions - help students UNDERSTAND the underlying concepts
- Connect topics to real-world applications, current events, and broader implications
- Provide multiple perspectives and approaches to understanding
- Ask thought-provoking questions that encourage deeper thinking
- Suggest connections between different subjects and topics
- Include analogies, examples, and visual descriptions to aid understanding
- Help students identify patterns, make predictions, and draw conclusions
- Address common misconceptions and learning challenges
- Provide study strategies, memory techniques, and learning tips
- Focus on the subject: {(subject ?? "general academic content")}
- Specific topic: {(topic ?? "as discussed")}
- Adapt your language to be suitable for both children and college-level students
- Be conversational, encouraging, and intellectually stimulating

EXPANSION APPROACH:
- Always add value beyond what's in the provided materials
- Help students see the ""big picture"" and how concepts connect
- Encourage critical thinking and independent learning
- Provide resources for further exploration
- Help students develop their own questions and curiosity
- Connect abstract concepts to concrete, relatable examples
- Show how concepts apply in different contexts and situations";
        }

        public int CountTokens(string text)
        {
            try
            {
                // Simple token estimation: ~4 characters per token for English text
                // This is a rough approximation - for production, use a proper tokenizer
                if (string.IsNullOrEmpty(text))
                    return 0;
                return Math.Max(1, text.Length / 4);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting tokens");
                return 0;
            }
        }

        public decimal CalculateCost(int inputTokens, int outputTokens)
        {
            try
            {
                // GPT-4 pricing (as of 2024) - adjust as needed
                var inputCostPer1K = 0.03m;  // $0.03 per 1K input tokens
                var outputCostPer1K = 0.06m; // $0.06 per 1K output tokens

                var inputCost = (inputTokens / 1000m) * inputCostPer1K;
                var outputCost = (outputTokens / 1000m) * outputCostPer1K;

                return inputCost + outputCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cost");
                return 0;
            }
        }

        public async Task<FileAnalysis> AnalyzeFileContentAsync(FileUpload file, string? subject = null, string? topic = null)
        {
            if (file == null)
            {
            return new FileAnalysis
            {
                Summary = "No file provided for analysis",
                KeyPoints = new List<string> { "No content to analyze" },
                Difficulty = "Unknown",
                Recommendations = new List<string> { "No topics identified" },
                Subject = subject ?? "Unknown",
                Topic = topic ?? "Unknown"
            };
            }

            var systemPrompt = BuildAnalysisSystemPrompt(subject, topic);
            var content = file.ProcessedContent ?? file.Content ?? $"[File: {file.FileName}, Type: {file.FileType}]";
            
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"Analyze this content:\n\n{content}" }
            };

            var response = await CallOpenAIAsync(messages);
            
            // Parse the response into FileAnalysis
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var parser = new AIResponseParser(loggerFactory.CreateLogger<AIResponseParser>());
            var analysis = parser.ParseFileAnalysis(response);
            analysis.FileId = file.Id;

            // Log token usage and cost
            var inputTokens = CountTokens(string.Join(" ", messages.Select(m => m.GetType().GetProperty("content")?.GetValue(m)?.ToString() ?? "")));
            var outputTokens = CountTokens(response);
            var cost = CalculateCost(inputTokens, outputTokens);

            _logger.LogInformation("File analysis completed - Input tokens: {InputTokens}, Output tokens: {OutputTokens}, Cost: ${Cost}", 
                inputTokens, outputTokens, cost);

            return analysis;
        }

        private string GenerateMockResponse(List<object> messages)
        {
            var lastMessage = messages.LastOrDefault();
            if (lastMessage == null) return "I'm sorry, I couldn't process your request.";

            var messageContent = lastMessage.GetType().GetProperty("content")?.GetValue(lastMessage)?.ToString() ?? "";
            
            if (messageContent.Contains("quiz", StringComparison.OrdinalIgnoreCase))
            {
                return @"{
  ""title"": ""Sample Quiz - Academic Content"",
  ""subject"": ""General Studies"",
  ""level"": ""Intermediate"",
  ""questions"": [
    {
      ""question"": ""What is the main topic discussed in the materials?"",
      ""options"": [""Mathematics"", ""Science"", ""History"", ""Literature""],
      ""correctAnswer"": ""Mathematics"",
      ""explanation"": ""The materials focus on mathematical concepts and problem-solving techniques.""
    },
    {
      ""question"": ""Which of the following is a key concept mentioned?"",
      ""options"": [""Photosynthesis"", ""Algebraic equations"", ""World War II"", ""Shakespeare's sonnets""],
      ""correctAnswer"": ""Algebraic equations"",
      ""explanation"": ""Algebraic equations are fundamental to the mathematical concepts covered in the materials.""
    },
    {
      ""question"": ""What study strategy is recommended?"",
      ""options"": [""Memorization only"", ""Regular practice"", ""Reading once"", ""Skipping examples""],
      ""correctAnswer"": ""Regular practice"",
      ""explanation"": ""Regular practice helps reinforce learning and improve understanding of the concepts.""
    }
  ]
}";
            }
            else if (messageContent.Contains("study guide", StringComparison.OrdinalIgnoreCase))
            {
                return @"{
  ""title"": ""Comprehensive Study Guide"",
  ""subject"": ""Academic Content"",
  ""level"": ""Intermediate"",
  ""content"": ""# Study Guide\n\n## Key Concepts\n\n### 1. Fundamental Principles\n- Core concept 1: Essential understanding\n- Core concept 2: Building blocks\n- Core concept 3: Advanced applications\n\n### 2. Important Formulas\n- Formula A: Basic calculation\n- Formula B: Advanced computation\n- Formula C: Problem-solving approach\n\n## Study Tips\n\n1. **Regular Review**: Review materials daily for better retention\n2. **Practice Problems**: Work through examples systematically\n3. **Create Flashcards**: Use flashcards for key terms and formulas\n4. **Group Study**: Collaborate with peers for different perspectives\n5. **Time Management**: Allocate specific study times for each topic\n\n## Practice Questions\n\n1. Explain the main concept discussed in the materials\n2. Solve the following problem using the provided formula\n3. Compare and contrast the different approaches mentioned\n4. Apply the concept to a real-world scenario\n\n## Additional Resources\n\n- Reference materials for deeper understanding\n- Online tutorials for visual learners\n- Practice tests for self-assessment\n\nThis study guide is based on your uploaded materials and will help you prepare effectively for your studies."",
  ""keyPoints"": [
    ""Core concept 1: Essential understanding"",
    ""Core concept 2: Building blocks"",
    ""Core concept 3: Advanced applications"",
    ""Regular review improves retention"",
    ""Practice problems reinforce learning"",
    ""Flashcards help with memorization""
  ],
  ""summary"": ""This study guide covers the fundamental concepts from your uploaded materials, providing a structured approach to learning with key points, study tips, and practice questions to enhance your understanding and retention.""
}";
            }
            else if (messageContent.Contains("analyze", StringComparison.OrdinalIgnoreCase) || messageContent.Contains("analysis", StringComparison.OrdinalIgnoreCase))
            {
                return @"{
  ""analysisType"": ""content_analysis"",
  ""subject"": ""Academic Content"",
  ""topic"": ""Educational Materials"",
  ""keyPoints"": [
    ""The content covers fundamental academic concepts"",
    ""Mathematical principles are prominently featured"",
    ""Problem-solving approaches are emphasized"",
    ""The material is suitable for intermediate learners"",
    ""Visual aids and examples are included""
  ],
  ""summary"": ""This content appears to be educational material focused on academic concepts, with an emphasis on mathematical principles and problem-solving techniques. The material is well-structured and suitable for intermediate-level learners."",
  ""difficulty"": ""intermediate"",
  ""recommendations"": [
    ""Review the fundamental concepts first"",
    ""Practice with the provided examples"",
    ""Create a study schedule for regular review"",
    ""Seek additional resources for clarification"",
    ""Form study groups for collaborative learning""
  ]
}";
            }
            else
            {
                return @"{
  ""title"": ""AI Study Assistant Response"",
  ""content"": ""I understand you're looking for help with your studies. Based on the materials you've provided, I can help you in several ways:\n\n## What I Can Do:\n\n1. **Create Study Guides**: I'll analyze your materials and create comprehensive study guides with key concepts, summaries, and study tips.\n\n2. **Generate Practice Quizzes**: I can create multiple-choice questions based on your content to test your understanding.\n\n3. **Analyze Content**: I'll examine your uploaded files and provide insights about the subject matter, difficulty level, and key topics.\n\n4. **Answer Questions**: Feel free to ask specific questions about the content, and I'll provide detailed explanations.\n\n## Getting Started:\n\n- Upload your study materials (PDFs, Word docs, images, etc.)\n- Ask me to create a study guide or quiz\n- Request analysis of specific content\n- Ask questions about any topic in your materials\n\nWhat would you like me to help you with today?"",
  ""keyPoints"": [
    ""Create comprehensive study guides"",
    ""Generate practice quizzes"",
    ""Analyze uploaded content"",
    ""Answer specific questions"",
    ""Provide study strategies""
  ],
  ""summary"": ""I'm your AI study assistant, ready to help you with study guides, quizzes, content analysis, and answering questions about your academic materials.""
}";
            }
        }

        private string BuildDynamicQuizSystemPrompt(string? subject, string? topic, int? questionCount, KnowledgeLevel? knowledgeLevel)
        {
            var targetQuestions = questionCount ?? 8;
            var level = knowledgeLevel ?? KnowledgeLevel.HighSchool;
            var levelDescription = GetLevelDescription(level);
            var questionDistribution = GetQuestionDistribution(targetQuestions, level);
            var estimatedTime = CalculateEstimatedTime(targetQuestions, level);

            return $@"You are an expert quiz generator creating {targetQuestions} questions for {levelDescription} level students.

Guidelines:
- Generate EXACTLY {targetQuestions} high-quality questions
- Target knowledge level: {levelDescription}
- Focus on subject: {(subject ?? "general academic content")}
- Specific topic: {(topic ?? "as requested by the user")}
- Vary question types: factual, analytical, application, synthesis
- Include 4 options (A, B, C, D) with plausible distractors
- Provide detailed explanations for correct answers
- Ensure questions test understanding, not just memorization

Question Distribution:
{questionDistribution}

Format as JSON with this structure:
{{
  ""title"": ""Appropriate Quiz Title for {levelDescription} Level"",
  ""knowledgeLevel"": ""{level}"",
  ""estimatedTime"": ""{estimatedTime} minutes"",
  ""questions"": [
    {{
      ""id"": 1,
      ""question"": ""Question text appropriate for {levelDescription} level"",
      ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
      ""correctAnswerIndex"": 0,
      ""explanation"": ""Detailed explanation with {levelDescription} level context"",
      ""difficulty"": ""medium"",
      ""bloomLevel"": ""apply""
    }}
  ]
}}";
        }

        private string GetLevelDescription(KnowledgeLevel level)
        {
            return level switch
            {
                KnowledgeLevel.Elementary => "Elementary (K-5)",
                KnowledgeLevel.MiddleSchool => "Middle School (6-8)",
                KnowledgeLevel.HighSchool => "High School (9-12)",
                KnowledgeLevel.College => "College/Undergraduate",
                KnowledgeLevel.Graduate => "Graduate/Professional",
                KnowledgeLevel.Expert => "Expert/Research",
                _ => "High School (9-12)"
            };
        }

        private string GetQuestionDistribution(int questionCount, KnowledgeLevel level)
        {
            var distribution = level switch
            {
                KnowledgeLevel.Elementary => "60% factual, 30% comprehension, 10% application",
                KnowledgeLevel.MiddleSchool => "40% factual, 40% comprehension, 20% application",
                KnowledgeLevel.HighSchool => "30% factual, 30% comprehension, 30% application, 10% analysis",
                KnowledgeLevel.College => "20% factual, 30% comprehension, 30% application, 20% analysis",
                KnowledgeLevel.Graduate => "10% factual, 20% comprehension, 30% application, 30% analysis, 10% synthesis",
                KnowledgeLevel.Expert => "5% factual, 15% comprehension, 25% application, 35% analysis, 20% synthesis",
                _ => "30% factual, 30% comprehension, 30% application, 10% analysis"
            };

            return $"- {distribution}";
        }

        private int CalculateEstimatedTime(int questionCount, KnowledgeLevel level)
        {
            var baseTime = questionCount; // 1 minute per question base
            var levelMultiplier = level switch
            {
                KnowledgeLevel.Elementary => 0.7,
                KnowledgeLevel.MiddleSchool => 0.8,
                KnowledgeLevel.HighSchool => 1.0,
                KnowledgeLevel.College => 1.2,
                KnowledgeLevel.Graduate => 1.5,
                KnowledgeLevel.Expert => 2.0,
                _ => 1.0
            };

            return (int)(baseTime * levelMultiplier);
        }
    }
}
