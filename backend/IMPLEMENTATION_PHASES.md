# **IMPLEMENTATION PHASES: Intelligent, Dynamic Study Platform**

## **Phase 1: User-Customizable File Grouping**

### **1.1 Database Schema Updates**

```sql
-- Add user customization fields to FileUploads
ALTER TABLE FileUploads ADD COLUMN AutoDetectedSubject VARCHAR(255);
ALTER TABLE FileUploads ADD COLUMN AutoDetectedTopic VARCHAR(255);
ALTER TABLE FileUploads ADD COLUMN UserDefinedSubject VARCHAR(255);
ALTER TABLE FileUploads ADD COLUMN UserDefinedTopic VARCHAR(255);
ALTER TABLE FileUploads ADD COLUMN IsUserModified BOOLEAN DEFAULT FALSE;
ALTER TABLE FileUploads ADD COLUMN SubjectGroupId INT;

-- Create custom subject groups table
CREATE TABLE SubjectGroups (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    GroupName VARCHAR(255) NOT NULL,
    Description TEXT,
    Color VARCHAR(7), -- Hex color for UI
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

### **1.2 API Endpoints**

- `PUT /api/files/{fileId}/subject` - Update file subject/topic
- `GET /api/files/{userId}/grouped` - Get files grouped by subject
- `POST /api/files/groups` - Create custom subject group
- `PUT /api/files/groups/{groupId}` - Update subject group
- `DELETE /api/files/groups/{groupId}` - Delete subject group
- `POST /api/files/bulk-update` - Bulk update multiple files

### **1.3 Backend Services**

- Enhanced `ContextService` with user customization support
- `SubjectGroupService` for managing custom groupings
- `FileGroupingService` for intelligent file organization

---

## **Phase 2: Dynamic Quiz Generation**

### **2.1 Content Analysis Service**

```csharp
public class ContentAnalysisService
{
    public ContentAnalysisResult AnalyzeContent(List<FileUpload> files)
    {
        return new ContentAnalysisResult
        {
            UniqueConcepts = ExtractUniqueConcepts(files),
            ComplexityScore = CalculateComplexity(files),
            ContentVolume = CalculateVolume(files),
            EstimatedQuestions = CalculateQuestionPotential(files),
            KnowledgeLevel = DetectKnowledgeLevel(files),
            TimeEstimate = EstimateQuizTime(files)
        };
    }
}
```

### **2.2 Dynamic Question Count Calculation**

```csharp
private int CalculateOptimalQuestionCount(ContentAnalysisResult analysis, QuizPreferences preferences)
{
    // Base calculation on content analysis
    var baseQuestions = Math.Max(3, Math.Min(50, analysis.UniqueConcepts * 0.8));

    // Apply multipliers
    var complexityMultiplier = Math.Max(0.5, Math.Min(2.0, analysis.ComplexityScore / 5.0));
    var userPreferenceMultiplier = preferences.LengthPreference switch
    {
        "quick" => 0.5,
        "standard" => 1.0,
        "comprehensive" => 1.8,
        "custom" => preferences.CustomMultiplier,
        _ => 1.0
    };

    var timeMultiplier = CalculateTimeMultiplier(preferences.TimeAvailable);

    return (int)(baseQuestions * complexityMultiplier * userPreferenceMultiplier * timeMultiplier);
}
```

### **2.3 Quiz Generation API Updates**

```csharp
[HttpPost("generate-quiz")]
public async Task<IActionResult> GenerateQuiz([FromBody] QuizRequest request)
{
    // Analyze content to determine optimal settings
    var contentAnalysis = await _contentAnalysisService.AnalyzeContent(request.SourceFiles);

    // Calculate dynamic question count
    var questionCount = CalculateOptimalQuestionCount(contentAnalysis, request.Preferences);

    // Generate quiz with dynamic sizing
    var quiz = await _analysisService.GenerateQuizAsync(
        request.UserPrompt,
        request.UserId,
        questionCount,
        contentAnalysis.KnowledgeLevel
    );
}
```

---

## **Phase 3: Knowledge Level Tracking**

### **3.1 Knowledge Level Detection**

```csharp
public enum KnowledgeLevel
{
    Elementary = 1,    // K-5
    MiddleSchool = 2,  // 6-8
    HighSchool = 3,    // 9-12
    College = 4,       // Undergraduate
    Graduate = 5,      // Graduate/Professional
    Expert = 6         // Advanced/Research
}

public class KnowledgeLevelDetector
{
    public KnowledgeLevel DetectLevel(List<FileUpload> files, UserPerformanceHistory history)
    {
        // Analyze file complexity
        var fileComplexity = AnalyzeFileComplexity(files);

        // Check user's historical performance
        var performanceLevel = AnalyzePerformanceHistory(history);

        // Look for academic indicators in content
        var academicIndicators = DetectAcademicLevel(files);

        // Combine factors to determine level
        return CombineFactors(fileComplexity, performanceLevel, academicIndicators);
    }
}
```

### **3.2 Database Schema for Knowledge Tracking**

```sql
-- User knowledge profiles
CREATE TABLE UserKnowledgeProfiles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Subject VARCHAR(255) NOT NULL,
    KnowledgeLevel INT NOT NULL,
    ConfidenceScore DECIMAL(3,2),
    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Quiz performance tracking
CREATE TABLE QuizPerformance (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    QuizId INT NOT NULL,
    Score DECIMAL(5,2),
    KnowledgeLevel INT,
    Difficulty INT,
    TimeSpent INT, -- seconds
    CompletedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (QuizId) REFERENCES Quizzes(Id)
);
```

---

## **Phase 4: Enhanced AI Prompts**

### **4.1 Dynamic Quiz Generation Prompt**

```csharp
private string BuildDynamicQuizSystemPrompt(string? subject, string? topic, int questionCount, KnowledgeLevel level)
{
    return $@"You are an expert quiz generator creating {questionCount} questions for {level} level students.

Guidelines:
- Generate EXACTLY {questionCount} high-quality questions
- Target knowledge level: {GetLevelDescription(level)}
- Focus on subject: {(subject ?? "general academic content")}
- Specific topic: {(topic ?? "as requested by the user")}
- Vary question types: factual, analytical, application, synthesis
- Include 4 options (A, B, C, D) with plausible distractors
- Provide detailed explanations for correct answers
- Ensure questions test understanding, not just memorization

Question Distribution:
- {GetQuestionDistribution(questionCount, level)}

Format as JSON with this structure:
{{
  ""title"": ""Appropriate Quiz Title for {level} Level"",
  ""knowledgeLevel"": ""{level}"",
  ""estimatedTime"": ""{CalculateEstimatedTime(questionCount, level)} minutes"",
  ""questions"": [
    {{
      ""id"": 1,
      ""question"": ""Question text appropriate for {level} level"",
      ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
      ""correctAnswerIndex"": 0,
      ""explanation"": ""Detailed explanation with {level} level context"",
      ""difficulty"": ""medium"",
      ""bloomLevel"": ""apply""
    }}
  ]
}}";
}
```

### **4.2 Enhanced Study Guide Prompts**

- Knowledge level-aware content generation
- Adaptive complexity based on user level
- Progressive difficulty within study guides
- Level-appropriate examples and explanations

---

## **Phase 5: User Interface Enhancements**

### **5.1 File Management Interface**

- **Drag-and-drop grouping** for files
- **Subject editing modal** with auto-suggestions
- **Bulk operations** for multiple files
- **Visual subject groups** with color coding
- **Search and filter** by subject, topic, or custom groups

### **5.2 Quiz Generation Interface**

- **Dynamic question count preview** based on content analysis
- **Knowledge level selector** with auto-detection
- **Time estimate display** for quiz completion
- **Custom preferences** for quiz length and difficulty
- **Real-time content analysis** showing what will be covered

### **5.3 Analytics Dashboard**

- **Knowledge progression** over time
- **Subject mastery levels** with visual indicators
- **Performance trends** and improvement areas
- **Learning recommendations** based on performance

---

## **Phase 6: Implementation Order**

### **Step 1: Database & Core Services (Week 1)**

1. Database schema updates
2. Content analysis service
3. Knowledge level detection
4. Dynamic question calculation

### **Step 2: API Development (Week 2)**

1. File grouping endpoints
2. Enhanced quiz generation
3. Knowledge tracking APIs
4. User preference management

### **Step 3: AI Integration (Week 3)**

1. Dynamic quiz prompts
2. Knowledge level-aware generation
3. Content analysis integration
4. Performance tracking

### **Step 4: Frontend Integration (Week 4)**

1. File management interface
2. Quiz generation UI
3. Analytics dashboard
4. User preference settings

---

## **Phase 7: Test Runner Integration & Backend Testing**

### **7.1 Update Test Runner HTML**

- Add new test sections for file grouping functionality
- Add tests for dynamic quiz generation
- Add tests for knowledge level detection
- Add tests for content analysis services

### **7.2 New Test Files to Create**

- `file-grouping.test.js` - Test file subject/topic management
- `content-analysis.test.js` - Test content analysis and question count calculation
- `knowledge-level.test.js` - Test knowledge level detection and tracking
- `dynamic-quiz.test.js` - Test dynamic quiz generation with various parameters
- `user-preferences.test.js` - Test user customization and preferences

### **7.3 Test Runner Enhancements**

```javascript
// Add to test-runner.html
<script src="file-grouping.test.js"></script>
<script src="content-analysis.test.js"></script>
<script src="knowledge-level.test.js"></script>
<script src="dynamic-quiz.test.js"></script>
<script src="user-preferences.test.js"></script>
```

### **7.4 Backend API Testing**

- Test all new endpoints with mock data
- Test file grouping with various subject combinations
- Test dynamic quiz generation with different content types
- Test knowledge level detection with sample files
- Test user customization workflows

### **7.5 Integration Testing**

- Test complete workflows from file upload to quiz generation
- Test user customization and override functionality
- Test knowledge level progression tracking
- Test content analysis accuracy with real file types

### **7.6 Performance Testing**

- Test with large numbers of files
- Test dynamic question calculation performance
- Test content analysis with complex documents
- Test knowledge level detection accuracy

---

## **Key Benefits of This Approach:**

1. **Truly Dynamic**: No hardcoded limits, adapts to content and user needs
2. **User Control**: Full customization while maintaining smart defaults
3. **Intelligent**: Uses content analysis and user history for better decisions
4. **Scalable**: Works for any content volume or complexity
5. **Personalized**: Adapts to individual learning levels and preferences
6. **Testable**: Comprehensive testing framework for all new functionality

**This creates a truly intelligent, adaptive study platform that grows with the user's needs while maintaining robust testing capabilities!**
