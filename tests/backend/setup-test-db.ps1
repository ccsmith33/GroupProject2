# Student Study AI - Test Database Setup
# This script sets up the test database for running tests

param(
    [string]$Password = "",
    [switch]$Help
)

if ($Help) {
    Write-Host "Usage: .\setup-test-db.ps1 -Password 'your_mysql_password'"
    Write-Host "This script creates the test database and user for running tests."
    exit 0
}

if ([string]::IsNullOrEmpty($Password)) {
    $Password = Read-Host "Enter your MySQL root password"
}

Write-Host "Setting up test database..." -ForegroundColor Green

# Test MySQL connection
try {
    $connectionString = "Server=localhost;Uid=root;Pwd=$Password;Port=3306;SslMode=Required;"
    $connection = New-Object MySql.Data.MySqlClient.MySqlConnection($connectionString)
    $connection.Open()
    Write-Host "MySQL connection successful" -ForegroundColor Green
    $connection.Close()
} catch {
    Write-Host "Failed to connect to MySQL: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure MySQL is running and the password is correct." -ForegroundColor Yellow
    exit 1
}

# Create test database and user
$sqlCommands = @(
    "CREATE DATABASE IF NOT EXISTS StudentStudyAI_Test;",
    "CREATE USER IF NOT EXISTS 'test'@'localhost' IDENTIFIED BY 'test';",
    "GRANT ALL PRIVILEGES ON StudentStudyAI_Test.* TO 'test'@'localhost';",
    "FLUSH PRIVILEGES;"
)

try {
    $connection = New-Object MySql.Data.MySqlClient.MySqlConnection($connectionString)
    $connection.Open()
    
    foreach ($sql in $sqlCommands) {
        $command = New-Object MySql.Data.MySqlClient.MySqlCommand($sql, $connection)
        $command.ExecuteNonQuery() | Out-Null
    }
    
    Write-Host "Test database and user created successfully" -ForegroundColor Green
    $connection.Close()
} catch {
    Write-Host "Failed to create test database: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Update test-settings.json
$testSettingsPath = "test-settings.json"
if (Test-Path $testSettingsPath) {
    $settings = Get-Content $testSettingsPath | ConvertFrom-Json
    $settings.TestDatabase.ConnectionString = "Server=localhost;Database=StudentStudyAI_Test;Uid=test;Pwd=test;Port=3306;SslMode=Required;"
    $settings | ConvertTo-Json -Depth 10 | Set-Content $testSettingsPath
    Write-Host "Updated test-settings.json" -ForegroundColor Green
} else {
    Write-Host "test-settings.json not found - you may need to update it manually" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Test database setup complete!" -ForegroundColor Green
Write-Host "You can now run tests with: .\run-tests.ps1" -ForegroundColor Cyan