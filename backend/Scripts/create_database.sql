-- Student Study AI Database Setup Script
-- Run this script to create the database and tables

-- Create database
CREATE DATABASE IF NOT EXISTS StudentStudyAI 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

-- Use the database
USE StudentStudyAI;

-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    LastLoginAt DATETIME NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    INDEX idx_username (Username),
    INDEX idx_email (Email)
);

-- Create guest user for non-authenticated access
INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt, LastLoginAt, IsActive)
VALUES (0, 'guest', 'guest@system.local', '', NOW(), NOW(), TRUE)
ON DUPLICATE KEY UPDATE Id=Id;

-- Create FileUploads table
CREATE TABLE IF NOT EXISTS FileUploads (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    FileType VARCHAR(10) NOT NULL,
    FileSize BIGINT NOT NULL,
    UploadedAt DATETIME NOT NULL,
    IsProcessed BOOLEAN NOT NULL DEFAULT FALSE,
    ProcessingStatus VARCHAR(50),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_id (UserId),
    INDEX idx_uploaded_at (UploadedAt)
);

-- Create AnalysisResults table
CREATE TABLE IF NOT EXISTS AnalysisResults (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OverallScore DECIMAL(5,2) NOT NULL,
    Feedback TEXT NOT NULL,
    StudyPlan TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    INDEX idx_created_at (CreatedAt)
);

-- Create StudySessions table
CREATE TABLE IF NOT EXISTS StudySessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    SessionName VARCHAR(100) NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NULL,
    Notes TEXT,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_id (UserId),
    INDEX idx_start_time (StartTime)
);

-- Create ChunkedFiles table
CREATE TABLE IF NOT EXISTS ChunkedFiles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FileUploadId INT NOT NULL,
    ChunkIndex INT NOT NULL,
    Content LONGTEXT NOT NULL,
    TokenCount INT NOT NULL,
    IsProcessed BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (FileUploadId) REFERENCES FileUploads(Id) ON DELETE CASCADE,
    INDEX idx_file_upload_id (FileUploadId),
    INDEX idx_chunk_index (ChunkIndex)
);

-- Insert sample data (optional)
INSERT INTO Users (Username, Email, PasswordHash, CreatedAt, LastLoginAt, IsActive) VALUES
('testuser', 'test@example.com', 'hashed_password_here', NOW(), NOW(), TRUE);

-- Show tables to verify creation
SHOW TABLES;
