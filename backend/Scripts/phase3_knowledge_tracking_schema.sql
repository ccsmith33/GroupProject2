-- Phase 3.2: Knowledge Tracking Database Schema
-- This script adds support for tracking user knowledge levels and performance

-- User knowledge profiles table
CREATE TABLE IF NOT EXISTS UserKnowledgeProfiles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Subject VARCHAR(255) NOT NULL,
    KnowledgeLevel INT NOT NULL,
    ConfidenceScore DECIMAL(3,2) DEFAULT 0.50,
    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_subject (UserId, Subject)
);

-- Quiz performance tracking table
CREATE TABLE IF NOT EXISTS QuizPerformance (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    QuizId INT NOT NULL,
    Score DECIMAL(5,2),
    KnowledgeLevel INT,
    Difficulty INT,
    TimeSpent INT, -- seconds
    CompletedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE
);

-- Knowledge progression history table
CREATE TABLE IF NOT EXISTS KnowledgeProgression (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Subject VARCHAR(255) NOT NULL,
    PreviousLevel INT,
    NewLevel INT,
    ChangeReason VARCHAR(255), -- 'quiz_performance', 'content_analysis', 'manual_adjustment'
    ConfidenceScore DECIMAL(3,2),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- User learning preferences table
CREATE TABLE IF NOT EXISTS UserLearningPreferences (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    PreferredQuizLength VARCHAR(50) DEFAULT 'standard', -- 'quick', 'standard', 'comprehensive', 'custom'
    CustomQuestionMultiplier DECIMAL(3,2) DEFAULT 1.0,
    PreferredDifficulty VARCHAR(50) DEFAULT 'adaptive', -- 'easy', 'medium', 'hard', 'adaptive'
    TimeAvailable INT DEFAULT 30, -- minutes
    StudyStyle VARCHAR(50) DEFAULT 'balanced', -- 'visual', 'auditory', 'kinesthetic', 'balanced'
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_preferences (UserId)
);

-- Content difficulty analysis table
CREATE TABLE IF NOT EXISTS ContentDifficultyAnalysis (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FileId INT NOT NULL,
    ComplexityScore DECIMAL(3,2),
    KnowledgeLevel INT,
    UniqueConcepts INT,
    ContentVolume INT,
    EstimatedQuestions INT,
    TimeEstimate INT, -- minutes
    AnalyzedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (FileId) REFERENCES FileUploads(Id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX idx_user_knowledge_profiles_user ON UserKnowledgeProfiles(UserId);
CREATE INDEX idx_user_knowledge_profiles_subject ON UserKnowledgeProfiles(Subject);
CREATE INDEX idx_quiz_performance_user ON QuizPerformance(UserId);
CREATE INDEX idx_quiz_performance_quiz ON QuizPerformance(QuizId);
CREATE INDEX idx_quiz_performance_completed ON QuizPerformance(CompletedAt);
CREATE INDEX idx_knowledge_progression_user ON KnowledgeProgression(UserId);
CREATE INDEX idx_knowledge_progression_subject ON KnowledgeProgression(Subject);
CREATE INDEX idx_content_difficulty_file ON ContentDifficultyAnalysis(FileId);

-- Insert default learning preferences for existing users
INSERT IGNORE INTO UserLearningPreferences (UserId, PreferredQuizLength, PreferredDifficulty, TimeAvailable, StudyStyle)
SELECT Id, 'standard', 'adaptive', 30, 'balanced'
FROM Users
WHERE Id NOT IN (SELECT UserId FROM UserLearningPreferences);
