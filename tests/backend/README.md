# Student Study AI - Backend Testing Suite

This comprehensive testing suite implements the complete testing strategy outlined in the `TEST_IMPLEMENTATION_PLAN.md` document.

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- MySQL 8.0+ (for integration tests)
- PowerShell (Windows) or Bash (Linux/macOS)

> **Note**: Docker is not required. Tests use direct MySQL connections.

### Running Tests

#### Windows (PowerShell)

```powershell
# Run all tests
.\run-tests.ps1

# Run specific test category
.\run-tests.ps1 -TestCategory "Unit"
.\run-tests.ps1 -TestCategory "Integration"
.\run-tests.ps1 -TestCategory "Performance"

# Run with coverage report
.\run-tests.ps1 -CoverageReport "true"
```

#### Linux/macOS (Bash)

```bash
# Make script executable (first time only)
chmod +x run-tests.sh

# Run all tests
./run-tests.sh

# Run specific test category
./run-tests.sh --category Unit
./run-tests.sh --category Integration
./run-tests.sh --category Performance

# Run with coverage report
./run-tests.sh --coverage true
```

#### Using dotnet CLI

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Performance"

# Run with coverage
dotnet test --collect "XPlat Code Coverage"
```

## 📋 Test Categories

### Phase 1: Core Infrastructure Tests

- **DatabaseServiceTests**: Connection management, CRUD operations, transaction handling
- **JwtServiceTests**: Token generation, validation, password hashing
- **FileStorageServiceTests**: File operations, content type detection, error handling

### Phase 2: File Processing Tests

- **FileProcessingServiceTests**: File validation, routing, pipeline orchestration
- **PdfProcessorTests**: PDF text/image extraction, metadata handling
- **WordProcessorTests**: Word document processing, formatting preservation
- **ImageProcessorTests**: OCR text extraction, image metadata
- **MediaProcessorTests**: Audio transcription, video frame extraction

### Phase 3: AI & Analysis Tests

- **OpenAIServiceTests**: API communication, token counting, cost calculation
- **AIResponseParserTests**: JSON/text response parsing, error handling
- **AnalysisServiceTests**: File analysis orchestration, caching behavior

### Phase 4: Controller Tests

- **AuthControllerTests**: User registration, login, token management
- **FileControllerTests**: File upload/download, validation, authorization
- **AnalysisControllerTests**: File analysis, study guide generation, quizzes
- **UserControllerTests**: Profile management, study sessions

### Phase 5: Integration Tests

- **ApiIntegrationTests**: End-to-end API workflows, error handling
- **DatabaseIntegrationTests**: Schema validation, data integrity, concurrent access

### Phase 6: Background Services Tests

- **BackgroundJobServiceTests**: Job queuing, processing, retry logic
- **CachedAnalysisServiceTests**: Cache hit/miss behavior, expiration

### Phase 7: Performance & Load Tests

- **PerformanceTests**: Response times, throughput, memory usage
- **LoadTests**: High-volume operations, concurrent access, stress testing

## 🏗️ Test Infrastructure

### Test Base Classes

- **TestBase**: Common test setup and utilities
- **DatabaseTestBase**: Database-specific test setup and cleanup
- **IntegrationTestBase**: Full application integration testing

### Mocking Strategy

- External APIs (OpenAI) - Always mocked
- File system - Mocked for unit tests, real for integration
- Database - Real for integration tests
- Background services - Mocked for unit tests

### Test Data Management

- **Bogus**: Realistic test data generation
- **AutoFixture**: Object generation for tests
- **Test data builders**: Complex scenario setup
- **Mock data factories**: Consistent test data

## 📊 Coverage and Quality

### Coverage Targets

- **Overall Coverage**: 90%+
- **Service Layer**: 95%+
- **Controller Layer**: 90%+
- **Critical Paths**: 100%

### Performance Targets

- **API Response Time**: < 200ms (95th percentile)
- **File Processing**: < 5s for 10MB files
- **Database Queries**: < 100ms average
- **Memory Usage**: < 500MB under load

### Quality Targets

- **Test Reliability**: 99%+ pass rate
- **Test Speed**: Complete suite < 5 minutes
- **Test Maintainability**: Clear, readable, well-documented

## 🔧 Configuration

### Test Settings (`test-settings.json`)

```json
{
  "TestDatabase": {
    "ConnectionString": "Server=localhost;Database=StudentStudyAI_Test;Uid=root;Pwd=YOUR_PASSWORD_HERE;Port=3306;SslMode=Required;",
    "UseInMemory": false
  },
  "OpenAI": {
    "ApiKey": "test-key",
    "BaseUrl": "https://api.openai.com/v1",
    "UseMock": true
  },
  "FileStorage": {
    "Path": "test-uploads",
    "UseInMemory": true
  }
}
```

### Environment Variables

- `TEST_DATABASE_CONNECTION`: Override test database connection
- `TEST_VERBOSE`: Enable verbose test output
- `TEST_COVERAGE`: Enable coverage reporting

## 🐛 Troubleshooting

### Common Issues

#### Database Connection Issues

**Windows:**

```powershell
# Check if MySQL service is running
Get-Service -Name "MySQL*"

# Start MySQL if not running
Start-Service -Name "MySQL80"

# Test connection
mysql -u root -p
```

**Linux/macOS:**

```bash
# Ensure MySQL is running
sudo systemctl start mysql

# Check connection string in test-settings.json
# Verify database exists and user has permissions
```

#### Test Failures

```bash
# Run with verbose output
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "FullyQualifiedName~DatabaseServiceTests"

# Check test results
cat TestResults/*.trx
```

#### Performance Test Issues

```bash
# Run performance tests separately
dotnet test --filter "Category=Performance"

# Increase timeout for slow tests
dotnet test --settings runsettings.xml
```

### Debugging

1. Set breakpoints in test methods
2. Use `dotnet test --logger "console;verbosity=detailed"`
3. Check test output in `TestResults/` directory
4. Review coverage report in `Coverage/` directory

## 📈 Continuous Integration

### GitHub Actions Example

```yaml
name: Backend Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    services:
      mysql:
        image: mysql:8.0
        env:
          MYSQL_ROOT_PASSWORD: root
          MYSQL_DATABASE: StudentStudyAI_Test
        ports:
          - 3306:3306
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: "8.0.x"
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --configuration Release
      - name: Test
        run: dotnet test --configuration Release --collect:"XPlat Code Coverage"
      - name: Upload coverage
        uses: codecov/codecov-action@v3
```

## 📚 Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Coverage Reporting](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)

## 🤝 Contributing

When adding new tests:

1. Follow the existing naming conventions
2. Use appropriate test categories
3. Include both positive and negative test cases
4. Add performance tests for critical paths
5. Update this documentation if needed

## 📝 License

This testing suite is part of the Student Study AI project and follows the same license terms.
