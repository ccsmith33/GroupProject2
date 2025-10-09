-- Update existing database schema for context-aware study guide system
-- Simplified version for better MySQL compatibility

USE StudentStudyAI;

-- Add new columns to existing FileUploads table for context management
ALTER TABLE FileUploads 
ADD COLUMN Content LONGTEXT,
ADD COLUMN ContextTags JSON,
ADD COLUMN ProcessedContent LONGTEXT,
ADD COLUMN TokenCount INT DEFAULT 0,
ADD COLUMN Subject VARCHAR(100),
ADD COLUMN Topic VARCHAR(200);

-- Create StudyGuides table
CREATE TABLE StudyGuides (
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
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create Conversations table
CREATE TABLE Conversations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Prompt TEXT NOT NULL,
    Response LONGTEXT NOT NULL,
    ContextFileIds JSON,
    ContextStudyGuideIds JSON,
    Subject VARCHAR(100),
    Topic VARCHAR(200),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create UserSessions table
CREATE TABLE UserSessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    SessionToken VARCHAR(255) NOT NULL UNIQUE,
    ContextWindow JSON,
    LastActivity DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create Quizzes table
CREATE TABLE Quizzes (
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
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create QuizAttempts table
CREATE TABLE QuizAttempts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    QuizId INT NOT NULL,
    UserId INT NOT NULL,
    Answers JSON NOT NULL,
    Score DECIMAL(5,2),
    CompletedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create FileStudyGuideLinks table
CREATE TABLE FileStudyGuideLinks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FileUploadId INT NOT NULL,
    StudyGuideId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (FileUploadId) REFERENCES FileUploads(Id) ON DELETE CASCADE,
    FOREIGN KEY (StudyGuideId) REFERENCES StudyGuides(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_file_study_guide (FileUploadId, StudyGuideId)
);

-- Update AnalysisResults table to link with users and files
ALTER TABLE AnalysisResults 
ADD COLUMN UserId INT,
ADD COLUMN FileUploadId INT,
ADD COLUMN Subject VARCHAR(100),
ADD COLUMN Topic VARCHAR(200),
ADD COLUMN UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

-- Add foreign key constraints for AnalysisResults
ALTER TABLE AnalysisResults 
ADD CONSTRAINT fk_analysis_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
ADD CONSTRAINT fk_analysis_file FOREIGN KEY (FileUploadId) REFERENCES FileUploads(Id) ON DELETE CASCADE;

-- Show updated tables
SHOW TABLES;
