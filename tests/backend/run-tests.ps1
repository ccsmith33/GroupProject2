# Student Study AI - Test Runner Script
# This script runs all tests with different configurations and generates reports

param(
    [string]$TestCategory = "All",
    [string]$OutputFormat = "console",
    [string]$CoverageReport = "true",
    [string]$Verbose = "false"
)

Write-Host "Student Study AI - Test Runner" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Set up test environment
$TestProject = "StudentStudyAI.Tests.csproj"
$TestResultsDir = "TestResults"
$CoverageDir = "Coverage"

# Create directories
if (!(Test-Path $TestResultsDir)) {
    New-Item -ItemType Directory -Path $TestResultsDir
}

if (!(Test-Path $CoverageDir)) {
    New-Item -ItemType Directory -Path $CoverageDir
}

# Build the test project
Write-Host "Building test project..." -ForegroundColor Yellow
dotnet build $TestProject --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Function to run tests with specific category
function Run-Tests {
    param(
        [string]$Category,
        [string]$Description,
        [string]$Filter = ""
    )
    
    Write-Host "`nRunning $Description..." -ForegroundColor Cyan
    
    $testArgs = @(
        "test",
        $TestProject,
        "--configuration", "Release",
        "--no-build",
        "--logger", "trx;LogFileName=$TestResultsDir\$Category-results.trx"
    )
    
    if ($Filter -ne "") {
        $testArgs += "--filter", $Filter
    }
    
    if ($CoverageReport -eq "true") {
        $testArgs += "--collect", "XPlat Code Coverage"
    }
    
    if ($Verbose -eq "true") {
        $testArgs += "--verbosity", "normal"
    } else {
        $testArgs += "--verbosity", "minimal"
    }
    
    & dotnet @testArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "$Description completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "$Description failed!" -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Run tests based on category
$allPassed = $true

switch ($TestCategory) {
    "All" {
        Write-Host "Running all tests..." -ForegroundColor Yellow
        
        # Phase 1: Core Infrastructure Tests
        $allPassed = (Run-Tests "Phase1" "Phase 1: Core Infrastructure Tests" "Category=Infrastructure") -and $allPassed
        
        # Phase 2: File Processing Tests
        $allPassed = (Run-Tests "Phase2" "Phase 2: File Processing Tests" "Category=FileProcessing") -and $allPassed
        
        # Phase 3: AI & Analysis Tests
        $allPassed = (Run-Tests "Phase3" "Phase 3: AI & Analysis Tests" "Category=AI") -and $allPassed
        
        # Phase 4: Controller Tests
        $allPassed = (Run-Tests "Phase4" "Phase 4: Controller Tests" "Category=Controllers") -and $allPassed
        
        # Phase 5: Integration Tests
        $allPassed = (Run-Tests "Phase5" "Phase 5: Integration Tests" "Category=Integration") -and $allPassed
        
        # Phase 6: Background Services Tests
        $allPassed = (Run-Tests "Phase6" "Phase 6: Background Services Tests" "Category=Background") -and $allPassed
        
        # Phase 7: Performance Tests
        $allPassed = (Run-Tests "Phase7" "Phase 7: Performance Tests" "Category=Performance") -and $allPassed
    }
    
    "Unit" {
        $allPassed = (Run-Tests "Unit" "Unit Tests" "Category=Unit") -and $allPassed
    }
    
    "Integration" {
        $allPassed = (Run-Tests "Integration" "Integration Tests" "Category=Integration") -and $allPassed
    }
    
    "Performance" {
        $allPassed = (Run-Tests "Performance" "Performance Tests" "Category=Performance") -and $allPassed
    }
    
    "Infrastructure" {
        $allPassed = (Run-Tests "Infrastructure" "Infrastructure Tests" "Category=Infrastructure") -and $allPassed
    }
    
    "FileProcessing" {
        $allPassed = (Run-Tests "FileProcessing" "File Processing Tests" "Category=FileProcessing") -and $allPassed
    }
    
    "AI" {
        $allPassed = (Run-Tests "AI" "AI & Analysis Tests" "Category=AI") -and $allPassed
    }
    
    "Controllers" {
        $allPassed = (Run-Tests "Controllers" "Controller Tests" "Category=Controllers") -and $allPassed
    }
    
    "Background" {
        $allPassed = (Run-Tests "Background" "Background Services Tests" "Category=Background") -and $allPassed
    }
    
    default {
        Write-Host "Unknown test category: $TestCategory" -ForegroundColor Red
        Write-Host "Available categories: All, Unit, Integration, Performance, Infrastructure, FileProcessing, AI, Controllers, Background" -ForegroundColor Yellow
        exit 1
    }
}

# Generate coverage report if requested
if ($CoverageReport -eq "true") {
    Write-Host "`nGenerating coverage report..." -ForegroundColor Yellow
    
    # Find coverage files
    $coverageFiles = Get-ChildItem -Path $TestResultsDir -Filter "coverage.cobertura.xml" -Recurse
    
    if ($coverageFiles.Count -gt 0) {
        # Install reportgenerator if not already installed
        $reportGenerator = "dotnet-reportgenerator-globaltool"
        $installed = dotnet tool list -g | Select-String $reportGenerator
        
        if (-not $installed) {
            Write-Host "Installing reportgenerator tool..." -ForegroundColor Yellow
            dotnet tool install -g $reportGenerator
        }
        
        # Generate HTML coverage report
        $coverageFile = $coverageFiles[0].FullName
        reportgenerator -reports:"$coverageFile" -targetdir:"$CoverageDir" -reporttypes:"Html"
        
        Write-Host "Coverage report generated in $CoverageDir" -ForegroundColor Green
    } else {
        Write-Host "No coverage files found" -ForegroundColor Yellow
    }
}

# Summary
Write-Host "`nTest Summary" -ForegroundColor Green
Write-Host "============" -ForegroundColor Green

if ($allPassed) {
    Write-Host "All tests passed! ✅" -ForegroundColor Green
    Write-Host "Test results are available in: $TestResultsDir" -ForegroundColor Cyan
    
    if ($CoverageReport -eq "true") {
        Write-Host "Coverage report is available in: $CoverageDir" -ForegroundColor Cyan
    }
    
    exit 0
} else {
    Write-Host "Some tests failed! ❌" -ForegroundColor Red
    Write-Host "Check the test results in: $TestResultsDir" -ForegroundColor Cyan
    exit 1
}
