-- Add admin field to Users table and create admin user
USE StudentStudyAI;

-- Add IsAdmin field to Users table
ALTER TABLE Users ADD COLUMN IsAdmin BOOLEAN NOT NULL DEFAULT FALSE;

-- Create admin user with ID 1
INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt, LastLoginAt, IsActive, IsAdmin) 
VALUES (1, 'admin', 'admin@studentstudyai.com', 'admin_password_hash', NOW(), NOW(), TRUE, TRUE);

-- Show the created user
SELECT * FROM Users WHERE Id = 1;
