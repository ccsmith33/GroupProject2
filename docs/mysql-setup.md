# MySQL Setup Guide

## Prerequisites

1. **Install MySQL Server 8.0.37**

   - Windows: Download from [MySQL Official Site](https://dev.mysql.com/downloads/mysql/) - Select version 8.0.37
   - macOS: `brew install mysql@8.0` (or download specific version)
   - Linux: `sudo apt-get install mysql-server-8.0` (Ubuntu/Debian)

2. **Install MySQL Workbench (Optional)**
   - Download from [MySQL Workbench](https://dev.mysql.com/downloads/workbench/)

## Database Setup

### 1. Start MySQL Service

```bash
# Windows (as Administrator)
net start mysql80

# macOS
brew services start mysql@8.0

# Linux
sudo systemctl start mysql
```

### 1.1. Verify MySQL 8.0.37 Installation

```bash
# Check MySQL version
mysql --version
# Should show: mysql Ver 8.0.37 for Win64 on x86_64 (MySQL Community Server - GPL)
```

### 2. Connect to MySQL

```bash
mysql -u root -p
```

### 3. Create Database and User

```sql
-- Create database
CREATE DATABASE StudentStudyAI CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user (optional - you can use root)
CREATE USER 'studentai'@'localhost' IDENTIFIED BY 'your_secure_password';
GRANT ALL PRIVILEGES ON StudentStudyAI.* TO 'studentai'@'localhost';
FLUSH PRIVILEGES;

-- Exit MySQL
EXIT;
```

### 4. Update Connection String

Update `backend/appsettings.json` with your MySQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentStudyAI;Uid=studentai;Pwd=your_secure_password;Port=3306;SslMode=Required;"
  }
}
```

**Note for MySQL 8.0.37:**

- SSL is required by default in MySQL 8.0+
- If you get SSL connection errors, you can use `SslMode=None;` for local development
- For production, always use `SslMode=Required;`

## Database Setup

### 1. Run SQL Script

```bash
# Connect to MySQL and run the setup script
mysql -u root -p < backend/Scripts/create_database.sql
```

### 2. Initialize Database from Application

The application will automatically create tables on first run using the DatabaseService.InitializeDatabaseAsync() method.

## Verification

### 1. Check Database Tables

```sql
USE StudentStudyAI;
SHOW TABLES;
```

You should see:

- Users
- FileUploads
- AnalysisResults
- StudySessions
- ChunkedFiles

### 2. Test Connection

Run the backend application:

```bash
cd backend
dotnet run
```

Check the health endpoint: `http://localhost:5000/health`

## Connecting to MySQL

### Command Line Connection

```bash
# Navigate to MySQL bin directory
cd C:\MySQL\mysql-8.0.37\bin

# Connect to MySQL (will prompt for password)
./mysql.exe -u root -p

# Or connect without password (if not set)
./mysql.exe -u root
```

### MySQL Workbench Connection

1. **Open MySQL Workbench**
2. **Click "+" next to "MySQL Connections"**
3. **Fill in connection details:**
   - **Connection Name**: `Local Instance 3306`
   - **Connection Method**: `Standard (TCP/IP)`
   - **Hostname**: `localhost`
   - **Port**: `3306`
   - **Username**: `root`
   - **Password**: `password123` (or your set password)
4. **Click "Test Connection"** to verify
5. **Click "OK"** to save and connect

### Check MySQL Server Status

```bash
# Check if MySQL is running
Get-Process | Where-Object {$_.ProcessName -eq "mysqld"}

# Check what port MySQL is using
netstat -an | findstr :3306
```

### Start MySQL Server (if not running)

```bash
# Start MySQL server
cd C:\MySQL\mysql-8.0.37\bin
Start-Process -FilePath ".\mysqld.exe" -ArgumentList "--datadir=C:\MySQL\data" -WindowStyle Hidden
```

## Troubleshooting

### Common Issues

1. **Connection Refused**

   - Ensure MySQL service is running
   - Check if port 3306 is open
   - Verify connection string credentials

2. **Authentication Failed**

   - Reset MySQL root password if needed
   - Ensure user has proper privileges

3. **Migration Errors**
   - Check if database exists
   - Verify connection string
   - Ensure all packages are installed

### Reset Database

```bash
# Drop and recreate database
mysql -u root -p -e "DROP DATABASE IF EXISTS StudentStudyAI; CREATE DATABASE StudentStudyAI CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# Re-run migrations
dotnet ef database update
```
