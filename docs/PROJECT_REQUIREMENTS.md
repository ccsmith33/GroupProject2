# Student Study AI Platform - Project Requirements

## Tech Stack

### Frontend

- **Technology**: Vanilla JavaScript + HTML
- **Styling**: CSS (Bootstrap or custom)
- **Additional**: Progressive Web App capabilities

### Backend

- **Language**: C#
- **Framework**: .NET 8
- **Database**: MySQL
- **ORM**: Entity Framework Core with Pomelo MySQL Provider

### AI/LLM

- **Primary Model**: GPT-4o
- **Pricing**: $0.005 per 1K input tokens, $0.015 per 1K output tokens
- **Budget**: $25/month target

## Core Features

- File upload (PowerPoints, notes, images, PDFs)
- AI-powered personalized feedback
- Grade tracking and analysis
- Study recommendations
- Collaborative features

## Project Structure

```
MIS 321 Group Project 2/
├── backend/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── FileController.cs
│   │   ├── AnalysisController.cs
│   │   └── UserController.cs
│   ├── Services/
│   │   ├── OpenAIService.cs
│   │   ├── FileProcessingService.cs
│   │   ├── DocumentProcessor.cs      // PDFs, Word docs
│   │   ├── ImageProcessor.cs         // Images, OCR
│   │   ├── VideoProcessor.cs         // Video analysis
│   │   ├── AudioProcessor.cs         // Speech-to-text
│   │   ├── AnalysisService.cs
│   │   ├── UserService.cs
│   │   └── FileValidator.cs          // Size, type validation
│   ├── Models/
│   │   ├── User.cs
│   │   ├── FileUpload.cs
│   │   ├── AnalysisResult.cs
│   │   ├── StudySession.cs
│   │   └── ChunkedFile.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   ├── Middleware/
│   │   ├── AuthenticationMiddleware.cs
│   │   └── FileUploadMiddleware.cs
│   ├── Utils/
│   │   ├── TokenCounter.cs
│   │   ├── FileTypeDetector.cs
│   │   ├── ChunkProcessor.cs         // Large file handling
│   │   ├── CompressionUtils.cs
│   │   └── CacheManager.cs
│   ├── Program.cs
│   └── appsettings.json
├── frontend/
│   ├── index.html
│   ├── css/
│   │   ├── main.css
│   │   └── components.css
│   ├── js/
│   │   ├── main.js
│   │   ├── api/
│   │   │   ├── auth.js
│   │   │   ├── files.js
│   │   │   └── analysis.js
│   │   ├── components/
│   │   │   ├── fileUpload.js
│   │   │   ├── dashboard.js
│   │   │   ├── feedback.js
│   │   │   └── progressTracker.js
│   │   ├── utils/
│   │   │   ├── fileHandler.js
│   │   │   ├── chunkProcessor.js     // Frontend chunking
│   │   │   └── ui.js
│   │   └── workers/
│   │       └── fileProcessor.js      // Web worker for heavy processing
│   └── assets/
│       ├── images/
│       └── icons/
├── tests/
│   ├── backend/
│   │   ├── Services/
│   │   └── Controllers/
│   └── frontend/
│       ├── components/
│       └── utils/
└── docs/
    ├── API.md
    ├── SETUP.md
    └── FILE_PROCESSING.md
```

## Team Collaboration

- **Frontend Developer**: Works in `frontend/` folder
- **Backend Developer**: Works in `backend/` folder
- **LLM Integration**: Focuses on `OpenAIService.cs` and `AnalysisService.cs`
- **Clear separation** prevents merge conflicts and allows parallel development

## Project Status

- **Phase**: Planning
- **Next Steps**: TBD
