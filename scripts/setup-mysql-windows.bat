@echo off
echo Setting up MySQL 8.0.37 for Student Study AI...

echo.
echo 1. Starting MySQL service...
net start mysql80
if %errorlevel% neq 0 (
    echo Failed to start MySQL service. Please check if MySQL 8.0.37 is installed.
    pause
    exit /b 1
)

echo.
echo 2. Creating database and tables...
mysql -u root -p < backend\Scripts\create_database.sql
if %errorlevel% neq 0 (
    echo Failed to create database. Please check your MySQL credentials.
    pause
    exit /b 1
)

echo.
echo 3. Testing connection...
mysql -u root -p -e "USE StudentStudyAI; SHOW TABLES;"
if %errorlevel% neq 0 (
    echo Failed to connect to database.
    pause
    exit /b 1
)

echo.
echo ✅ MySQL setup completed successfully!
echo.
echo Next steps:
echo 1. Update backend\appsettings.json with your MySQL password
echo 2. Run: cd backend && dotnet run
echo 3. Test: curl -X POST http://localhost:5000/init-db
echo.
pause
