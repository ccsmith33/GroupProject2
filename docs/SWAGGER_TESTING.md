# Swagger UI Testing Guide

This document outlines the comprehensive testing strategy for Swagger UI in the Music Lesson Management API.

## Overview

We have implemented multiple layers of Swagger UI testing to ensure:

- OpenAPI specification validity
- UI functionality and usability
- Visual consistency
- Contract compliance
- End-to-end user experience

## Test Categories

### 1. Swagger Validation Tests (`SwaggerValidationTests.cs`)

**Purpose**: Validates the OpenAPI specification and basic Swagger functionality.

**Tests**:

- ✅ Swagger JSON endpoint accessibility
- ✅ Swagger UI page loading
- ✅ All controller endpoints present
- ✅ Valid schema definitions
- ✅ Proper HTTP methods
- ✅ Valid response codes

**Run**: `dotnet test --filter "FullyQualifiedName~SwaggerValidationTests"`

### 2. OpenAPI Contract Tests (`OpenAPIContractTests.cs`)

**Purpose**: Validates OpenAPI 3.0 specification compliance and contract integrity.

**Tests**:

- ✅ Valid OpenAPI 3.0 format
- ✅ Complete API information
- ✅ Consistent schema references
- ✅ Valid security schemes
- ✅ Proper response codes
- ✅ Consistent parameter types

**Run**: `dotnet test --filter "FullyQualifiedName~OpenAPIContractTests"`

### 3. Swagger UI Tests (`SwaggerUITests.cs`)

**Purpose**: End-to-end testing of Swagger UI functionality using Playwright.

**Tests**:

- ✅ Swagger UI loads successfully
- ✅ All endpoints displayed
- ✅ Endpoint expansion works
- ✅ Try it out functionality
- ✅ Model schemas visible
- ✅ Authentication flow
- ✅ Responsive design

**Run**: `dotnet test --filter "FullyQualifiedName~SwaggerUITests"`

**Prerequisites**: Playwright browsers installed

```bash
dotnet tool install --global Microsoft.Playwright.CLI
playwright install chromium
```

### 4. Visual Regression Tests (`SwaggerVisualTests.cs`)

**Purpose**: Ensures visual consistency and proper rendering across different screen sizes.

**Tests**:

- ✅ Layout consistency
- ✅ CSS styling applied
- ✅ Responsive design (Desktop/Tablet/Mobile)
- ✅ Dark mode toggle (if available)
- ✅ Error state display

**Run**: `dotnet test --filter "FullyQualifiedName~SwaggerVisualTests"`

## Running Tests

### All Swagger Tests

```bash
dotnet test --filter "FullyQualifiedName~Swagger"
```

### Specific Test Categories

```bash
# Validation only
dotnet test --filter "FullyQualifiedName~SwaggerValidationTests"

# Contract testing only
dotnet test --filter "FullyQualifiedName~OpenAPIContractTests"

# UI testing only (requires Playwright)
dotnet test --filter "FullyQualifiedName~SwaggerUITests"

# Visual testing only (requires Playwright)
dotnet test --filter "FullyQualifiedName~SwaggerVisualTests"
```

### Using the PowerShell Script

```powershell
# Run all tests
.\scripts\run-swagger-tests.ps1

# Run with Playwright UI tests
.\scripts\run-swagger-tests.ps1 -Playwright

# Run with visual regression tests
.\scripts\run-swagger-tests.ps1 -Playwright -Visual

# Verbose output
.\scripts\run-swagger-tests.ps1 -Verbose
```

## Test Configuration

### Swagger Test Factory

The `SwaggerTestConfiguration.cs` provides a specialized test factory that:

- Enables Swagger in test environment
- Configures in-memory database
- Sets up proper test configuration

### Environment Variables

```json
{
  "ConnectionStrings:DefaultConnection": "Data Source=:memory:",
  "ASPNETCORE_ENVIRONMENT": "Development",
  "SkipDatabaseInit": "true"
}
```

## CI/CD Integration

### GitHub Actions

The `.github/workflows/swagger-tests.yml` workflow:

1. Runs validation tests on every push/PR
2. Runs UI tests with Playwright
3. Generates test reports
4. Uploads artifacts

### Test Reports

- **Validation Results**: `swagger-validation.trx`
- **Contract Results**: `openapi-contract.trx`
- **UI Results**: `swagger-ui.trx`
- **Visual Results**: `swagger-visual.trx`

## Best Practices

### 1. Test Data Management

- Use in-memory database for tests
- Mock external dependencies
- Clean up after each test

### 2. Visual Testing

- Take baseline screenshots
- Test on multiple screen sizes
- Handle dynamic content appropriately

### 3. Performance

- Run UI tests in headless mode for CI
- Use parallel test execution where possible
- Cache Playwright browsers

### 4. Maintenance

- Update tests when API changes
- Keep visual baselines current
- Monitor test execution time

## Troubleshooting

### Common Issues

1. **Playwright Not Found**

   ```bash
   dotnet tool install --global Microsoft.Playwright.CLI
   playwright install chromium
   ```

2. **Swagger UI Not Loading**

   - Check if API is running
   - Verify Swagger configuration
   - Check CORS settings

3. **Visual Test Failures**

   - Update baseline images
   - Check for dynamic content
   - Verify consistent test environment

4. **Contract Test Failures**
   - Update OpenAPI specification
   - Check schema references
   - Verify response codes

### Debug Mode

```bash
# Run with detailed logging
dotnet test --filter "FullyQualifiedName~Swagger" --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~SwaggerValidationTests.SwaggerJson_ShouldBeValid"
```

## Future Enhancements

1. **API Documentation Testing**

   - Validate example responses
   - Test parameter descriptions
   - Verify error message clarity

2. **Performance Testing**

   - Load test Swagger UI
   - Measure response times
   - Test concurrent users

3. **Accessibility Testing**

   - Screen reader compatibility
   - Keyboard navigation
   - Color contrast validation

4. **Internationalization**
   - Multi-language support
   - RTL layout testing
   - Localized error messages

## Resources

- [OpenAPI Specification](https://swagger.io/specification/)
- [Swashbuckle Documentation](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle)
- [Playwright .NET](https://playwright.dev/dotnet/)
- [xUnit Testing](https://xunit.net/)
