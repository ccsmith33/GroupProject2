#!/bin/bash

# Student Study AI - Test Runner Script (Linux/macOS)
# This script runs all tests with different configurations and generates reports

# Default parameters
TEST_CATEGORY="All"
OUTPUT_FORMAT="console"
COVERAGE_REPORT="true"
VERBOSE="false"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --category)
            TEST_CATEGORY="$2"
            shift 2
            ;;
        --output)
            OUTPUT_FORMAT="$2"
            shift 2
            ;;
        --coverage)
            COVERAGE_REPORT="$2"
            shift 2
            ;;
        --verbose)
            VERBOSE="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  --category CATEGORY    Test category to run (All, Unit, Integration, Performance, etc.)"
            echo "  --output FORMAT        Output format (console, trx, etc.)"
            echo "  --coverage BOOLEAN     Generate coverage report (true/false)"
            echo "  --verbose BOOLEAN      Verbose output (true/false)"
            echo "  -h, --help            Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "Student Study AI - Test Runner"
echo "=============================="

# Set up test environment
TEST_PROJECT="StudentStudyAI.Tests.csproj"
TEST_RESULTS_DIR="TestResults"
COVERAGE_DIR="Coverage"

# Create directories
mkdir -p "$TEST_RESULTS_DIR"
mkdir -p "$COVERAGE_DIR"

# Build the test project
echo "Building test project..."
dotnet build "$TEST_PROJECT" --configuration Release

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Function to run tests with specific category
run_tests() {
    local category="$1"
    local description="$2"
    local filter="$3"
    
    echo ""
    echo "Running $description..."
    
    local test_args=(
        "test"
        "$TEST_PROJECT"
        "--configuration" "Release"
        "--no-build"
        "--logger" "trx;LogFileName=$TEST_RESULTS_DIR/$category-results.trx"
    )
    
    if [ -n "$filter" ]; then
        test_args+=("--filter" "$filter")
    fi
    
    if [ "$COVERAGE_REPORT" = "true" ]; then
        test_args+=("--collect" "XPlat Code Coverage")
    fi
    
    if [ "$VERBOSE" = "true" ]; then
        test_args+=("--verbosity" "normal")
    else
        test_args+=("--verbosity" "minimal")
    fi
    
    dotnet "${test_args[@]}"
    
    if [ $? -eq 0 ]; then
        echo "$description completed successfully!"
        return 0
    else
        echo "$description failed!"
        return 1
    fi
}

# Run tests based on category
ALL_PASSED=true

case "$TEST_CATEGORY" in
    "All")
        echo "Running all tests..."
        
        # Phase 1: Core Infrastructure Tests
        run_tests "Phase1" "Phase 1: Core Infrastructure Tests" "Category=Infrastructure" || ALL_PASSED=false
        
        # Phase 2: File Processing Tests
        run_tests "Phase2" "Phase 2: File Processing Tests" "Category=FileProcessing" || ALL_PASSED=false
        
        # Phase 3: AI & Analysis Tests
        run_tests "Phase3" "Phase 3: AI & Analysis Tests" "Category=AI" || ALL_PASSED=false
        
        # Phase 4: Controller Tests
        run_tests "Phase4" "Phase 4: Controller Tests" "Category=Controllers" || ALL_PASSED=false
        
        # Phase 5: Integration Tests
        run_tests "Phase5" "Phase 5: Integration Tests" "Category=Integration" || ALL_PASSED=false
        
        # Phase 6: Background Services Tests
        run_tests "Phase6" "Phase 6: Background Services Tests" "Category=Background" || ALL_PASSED=false
        
        # Phase 7: Performance Tests
        run_tests "Phase7" "Phase 7: Performance Tests" "Category=Performance" || ALL_PASSED=false
        ;;
    
    "Unit")
        run_tests "Unit" "Unit Tests" "Category=Unit" || ALL_PASSED=false
        ;;
    
    "Integration")
        run_tests "Integration" "Integration Tests" "Category=Integration" || ALL_PASSED=false
        ;;
    
    "Performance")
        run_tests "Performance" "Performance Tests" "Category=Performance" || ALL_PASSED=false
        ;;
    
    "Infrastructure")
        run_tests "Infrastructure" "Infrastructure Tests" "Category=Infrastructure" || ALL_PASSED=false
        ;;
    
    "FileProcessing")
        run_tests "FileProcessing" "File Processing Tests" "Category=FileProcessing" || ALL_PASSED=false
        ;;
    
    "AI")
        run_tests "AI" "AI & Analysis Tests" "Category=AI" || ALL_PASSED=false
        ;;
    
    "Controllers")
        run_tests "Controllers" "Controller Tests" "Category=Controllers" || ALL_PASSED=false
        ;;
    
    "Background")
        run_tests "Background" "Background Services Tests" "Category=Background" || ALL_PASSED=false
        ;;
    
    *)
        echo "Unknown test category: $TEST_CATEGORY"
        echo "Available categories: All, Unit, Integration, Performance, Infrastructure, FileProcessing, AI, Controllers, Background"
        exit 1
        ;;
esac

# Generate coverage report if requested
if [ "$COVERAGE_REPORT" = "true" ]; then
    echo ""
    echo "Generating coverage report..."
    
    # Find coverage files
    COVERAGE_FILES=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" -type f)
    
    if [ -n "$COVERAGE_FILES" ]; then
        # Install reportgenerator if not already installed
        REPORT_GENERATOR="dotnet-reportgenerator-globaltool"
        INSTALLED=$(dotnet tool list -g | grep -q "$REPORT_GENERATOR" && echo "true" || echo "false")
        
        if [ "$INSTALLED" = "false" ]; then
            echo "Installing reportgenerator tool..."
            dotnet tool install -g "$REPORT_GENERATOR"
        fi
        
        # Generate HTML coverage report
        COVERAGE_FILE=$(echo "$COVERAGE_FILES" | head -n1)
        reportgenerator -reports:"$COVERAGE_FILE" -targetdir:"$COVERAGE_DIR" -reporttypes:"Html"
        
        echo "Coverage report generated in $COVERAGE_DIR"
    else
        echo "No coverage files found"
    fi
fi

# Summary
echo ""
echo "Test Summary"
echo "============"

if [ "$ALL_PASSED" = true ]; then
    echo "All tests passed! ✅"
    echo "Test results are available in: $TEST_RESULTS_DIR"
    
    if [ "$COVERAGE_REPORT" = "true" ]; then
        echo "Coverage report is available in: $COVERAGE_DIR"
    fi
    
    exit 0
else
    echo "Some tests failed! ❌"
    echo "Check the test results in: $TEST_RESULTS_DIR"
    exit 1
fi
