# MySQL 8.0.37 Setup Script for Student Study AI
Write-Host "Setting up MySQL 8.0.37 for Student Study AI..." -ForegroundColor Green

# Check if MySQL is installed
try {
    $mysqlVersion = mysql --version 2>$null
    if ($mysqlVersion -match "8\.0\.37") {
        Write-Host "✅ MySQL 8.0.37 detected" -ForegroundColor Green
    } else {
        Write-Host "⚠️  MySQL version mismatch. Expected 8.0.37, found: $mysqlVersion" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ MySQL not found in PATH. Please install MySQL 8.0.37 first." -ForegroundColor Red
    exit 1
}

# Start MySQL service
Write-Host "`n1. Starting MySQL service..." -ForegroundColor Cyan
try {
    Start-Service -Name "mysql80" -ErrorAction Stop
    Write-Host "✅ MySQL service started" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to start MySQL service. Please check if MySQL 8.0.37 is installed." -ForegroundColor Red
    exit 1
}

# Create database
Write-Host "`n2. Creating database and tables..." -ForegroundColor Cyan
$password = Read-Host "Enter MySQL root password" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

try {
    $env:MYSQL_PWD = $plainPassword
    mysql -u root -e "source backend\Scripts\create_database.sql"
    Write-Host "✅ Database created successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create database. Please check your MySQL credentials." -ForegroundColor Red
    exit 1
}

# Test connection
Write-Host "`n3. Testing connection..." -ForegroundColor Cyan
try {
    mysql -u root -e "USE StudentStudyAI; SHOW TABLES;"
    Write-Host "✅ Database connection successful" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to connect to database." -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 MySQL setup completed successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Update backend\appsettings.json with your MySQL password" -ForegroundColor White
Write-Host "2. Run: cd backend && dotnet run" -ForegroundColor White
Write-Host "3. Test: curl -X POST http://localhost:5000/init-db" -ForegroundColor White

# Clean up
Remove-Item Env:MYSQL_PWD
