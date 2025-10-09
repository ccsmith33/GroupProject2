-- Update existing database schema for context-aware study guide system
-- Run this script to add new tables and modify existing ones

USE StudentStudyAI;

-- Add new columns to existing FileUploads table for context management
ALTER TABLE FileUploads 
ADD COLUMN Content LONGTEXT,
ADD COLUMN ContextTags JSON,
ADD COLUMN ProcessedContent LONGTEXT,
ADD COLUMN TokenCount INT DEFAULT 0,
ADD COLUMN Subject VARCHAR(100),
ADD COLUMN Topic VARCHAR(200);

-- Add indexes for better performance
CREATE INDEX idx_file_content ON FileUploads(Content(100));
CREATE INDEX idx_file_subject ON FileUploads(Subject);
CREATE INDEX idx_file_topic ON FileUploads(Topic);
CREATE INDEX idx_file_uploaded_at ON FileUploads(UploadedAt);

-- Create StudyGuides table for storing generated study guides
CREATE TABLE IF NOT EXISTS StudyGuides (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Content LONGTEXT NOT NULL,
    Subject VARCHAR(100),
    Topic VARCHAR(200),
    SourceFileIds JSON,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_id (UserId),
    INDEX idx_subject (Subject),
    INDEX idx_topic (Topic),
    INDEX idx_created_at (CreatedAt)
);

-- Create Conversations table for tracking user prompts and AI responses
CREATE TABLE IF NOT EXISTS Conversations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Prompt TEXT NOT NULL,
    Response LONGTEXT NOT NULL,
    ContextFileIds JSON,
    ContextStudyGuideIds JSON,
    Subject VARCHAR(100),
    Topic VARCHAR(200),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_id (UserId),
    INDEX idx_subject (Subject),
    INDEX idx_topic (Topic),
    INDEX idx_created_at (CreatedAt)
);

-- Create UserSessions table for tracking active user sessions and context
CREATE TABLE IF NOT EXISTS UserSessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    SessionToken VARCHAR(255) NOT NULL UNIQUE,
    ContextWindow JSON,
    LastActivity DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_id (UserId),
    INDEX idx_session_token (SessionToken),
    INDEX idx_last_activity (LastActivity),
    INDEX idx_expires_at (ExpiresAt)
);

-- Create FileStudyGuideLinks table for many-to-many relationship
CREATE TABLE IF NOT EXISTS FileStudyGuideLinks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FileUploadId INT NOT NULL,
    StudyGuideId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (FileUploadId) REFERENCES FileUploads(Id) ON DELETE CASCADE,
    FOREIGN KEY (StudyGuideId) REFERENCES StudyGuides(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_file_study_guide (FileUploadId, StudyGuideId),
    INDEX idx_file_upload_id (FileUploadId),
    INDEX idx_study_guide_id (StudyGuideId)
);

-- Create Quizzes table for storing generated quizzes
CREATE TABLE IF NOT EXISTS Quizzes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Questions JSON NOT NULL,
    SourceFileIds JSON,
    SourceStudyGuideIds JSON,
    Subject VARCHAR(100),
    Topic VARCHAR(200),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_id (UserId),
    INDEX idx_subject (Subject),
    INDEX idx_topic (Topic),
    INDEX idx_created_at (CreatedAt)
);

-- Create QuizAttempts table for tracking quiz attempts
CREATE TABLE IF NOT EXISTS QuizAttempts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    QuizId INT NOT NULL,
    UserId INT NOT NULL,
    Answers JSON NOT NULL,
    Score DECIMAL(5,2),
    CompletedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_quiz_id (QuizId),
    INDEX idx_user_id (UserId),
    INDEX idx_completed_at (CompletedAt)
);

-- Update AnalysisResults table to link with users and files
ALTER TABLE AnalysisResults 
ADD COLUMN IF NOT EXISTS UserId INT,
ADD COLUMN IF NOT EXISTS FileUploadId INT,
ADD COLUMN IF NOT EXISTS Subject VARCHAR(100),
ADD COLUMN IF NOT EXISTS Topic VARCHAR(200),
ADD COLUMN IF NOT EXISTS UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

-- Add foreign key constraints for AnalysisResults
ALTER TABLE AnalysisResults 
ADD CONSTRAINT fk_analysis_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
ADD CONSTRAINT fk_analysis_file FOREIGN KEY (FileUploadId) REFERENCES FileUploads(Id) ON DELETE CASCADE;

-- Add indexes for AnalysisResults
CREATE INDEX idx_analysis_user_id ON AnalysisResults(UserId);
CREATE INDEX idx_analysis_file_id ON AnalysisResults(FileUploadId);
CREATE INDEX idx_analysis_subject ON AnalysisResults(Subject);
CREATE INDEX idx_analysis_topic ON AnalysisResults(Topic);

-- Create a view for easy context retrieval
CREATE OR REPLACE VIEW UserContextView AS
SELECT 
    u.Id as UserId,
    u.Username,
    f.Id as FileId,
    f.FileName,
    f.Subject,
    f.Topic,
    f.ContextTags,
    f.UploadedAt,
    sg.Id as StudyGuideId,
    sg.Title as StudyGuideTitle,
    sg.CreatedAt as StudyGuideCreatedAt,
    c.Id as ConversationId,
    c.Prompt,
    c.CreatedAt as ConversationCreatedAt
FROM Users u
LEFT JOIN FileUploads f ON u.Id = f.UserId
LEFT JOIN FileStudyGuideLinks fsl ON f.Id = fsl.FileUploadId
LEFT JOIN StudyGuides sg ON fsl.StudyGuideId = sg.Id
LEFT JOIN Conversations c ON u.Id = c.UserId
WHERE u.IsActive = TRUE
ORDER BY f.UploadedAt DESC, sg.CreatedAt DESC, c.CreatedAt DESC;

-- Show updated tables
SHOW TABLES;
