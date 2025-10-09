-- Phase 1: User-Customizable File Grouping Database Schema Updates
-- This script adds support for intelligent file grouping and user customization

-- Add user customization fields to FileUploads table
ALTER TABLE FileUploads 
ADD COLUMN AutoDetectedSubject VARCHAR(255),
ADD COLUMN AutoDetectedTopic VARCHAR(255),
ADD COLUMN UserDefinedSubject VARCHAR(255),
ADD COLUMN UserDefinedTopic VARCHAR(255),
ADD COLUMN IsUserModified BOOLEAN DEFAULT FALSE,
ADD COLUMN SubjectGroupId INT;

-- Create custom subject groups table
CREATE TABLE IF NOT EXISTS SubjectGroups (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    GroupName VARCHAR(255) NOT NULL,
    Description TEXT,
    Color VARCHAR(7) DEFAULT '#3498db', -- Hex color for UI
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_group (UserId, GroupName)
);

-- Create index for better performance
CREATE INDEX idx_fileuploads_subject_group ON FileUploads(SubjectGroupId);
CREATE INDEX idx_fileuploads_auto_subject ON FileUploads(AutoDetectedSubject);
CREATE INDEX idx_fileuploads_user_subject ON FileUploads(UserDefinedSubject);
CREATE INDEX idx_subjectgroups_user ON SubjectGroups(UserId);

-- Update existing files to have auto-detected subjects (will be populated by processing)
UPDATE FileUploads 
SET AutoDetectedSubject = Subject, 
    AutoDetectedTopic = Topic 
WHERE Subject IS NOT NULL AND AutoDetectedSubject IS NULL;
