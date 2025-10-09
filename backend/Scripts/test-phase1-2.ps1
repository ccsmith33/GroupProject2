# Phase 1-2 Backend Test Script
# Tests the new file grouping and dynamic quiz functionality

Write-Host "Phase 1-2 Backend Test Script" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

$baseUrl = "http://localhost:5000/api"

# Test 1: Check if API is running
Write-Host "`n1. Testing API availability..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method GET -ErrorAction Stop
    Write-Host "✅ API is running" -ForegroundColor Green
} catch {
    Write-Host "❌ API is not running. Please start the backend first." -ForegroundColor Red
    exit 1
}

# Test 2: Test file grouping endpoints
Write-Host "`n2. Testing file grouping endpoints..." -ForegroundColor Yellow

# Test get grouped files
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/files/1/grouped" -Method GET -ErrorAction Stop
    Write-Host "✅ Get grouped files endpoint working" -ForegroundColor Green
} catch {
    Write-Host "❌ Get grouped files endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test get subject groups
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/files/groups?userId=1" -Method GET -ErrorAction Stop
    Write-Host "✅ Get subject groups endpoint working" -ForegroundColor Green
} catch {
    Write-Host "❌ Get subject groups endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Test content analysis (via quiz generation)
Write-Host "`n3. Testing dynamic quiz generation..." -ForegroundColor Yellow

$quizRequest = @{
    userPrompt = "Create a test quiz about mathematics"
    userId = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/analysis/generate-quiz" -Method POST -Body $quizRequest -ContentType "application/json" -ErrorAction Stop
    Write-Host "✅ Dynamic quiz generation working" -ForegroundColor Green
    Write-Host "   Quiz title: $($response.data.title)" -ForegroundColor Cyan
    Write-Host "   Subject: $($response.data.subject)" -ForegroundColor Cyan
    Write-Host "   Questions: $((ConvertFrom-Json $response.data.questions).Count)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Dynamic quiz generation failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Test subject group creation
Write-Host "`n4. Testing subject group creation..." -ForegroundColor Yellow

$groupRequest = @{
    groupName = "Test Group $(Get-Date -Format 'yyyyMMddHHmmss')"
    description = "Test group created by PowerShell script"
    color = "#ff6b6b"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/files/groups" -Method POST -Body $groupRequest -ContentType "application/json" -ErrorAction Stop
    Write-Host "✅ Subject group creation working" -ForegroundColor Green
    Write-Host "   Group ID: $($response.data.id)" -ForegroundColor Cyan
    Write-Host "   Group Name: $($response.data.groupName)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Subject group creation failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Test study guide generation
Write-Host "`n5. Testing study guide generation..." -ForegroundColor Yellow

$studyGuideRequest = @{
    userPrompt = "Create a study guide about calculus"
    userId = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/analysis/generate-study-guide" -Method POST -Body $studyGuideRequest -ContentType "application/json" -ErrorAction Stop
    Write-Host "✅ Study guide generation working" -ForegroundColor Green
    Write-Host "   Study guide title: $($response.data.title)" -ForegroundColor Cyan
    Write-Host "   Subject: $($response.data.subject)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Study guide generation failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=================================" -ForegroundColor Green
Write-Host "Phase 1-2 Backend Test Complete!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

Write-Host "`nTo test the full functionality:" -ForegroundColor Cyan
Write-Host "1. Open http://localhost:5000/test-phase1-2.html in your browser" -ForegroundColor White
Write-Host "2. Upload some files to test auto-detection" -ForegroundColor White
Write-Host "3. Test file grouping and subject management" -ForegroundColor White
Write-Host "4. Generate dynamic quizzes with different content" -ForegroundColor White
