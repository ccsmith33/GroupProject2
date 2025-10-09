using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Services;
using Xunit;

namespace StudentStudyAI.Tests.Infrastructure;

public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected DatabaseService? DatabaseService { get; private set; }
    protected IConfiguration? Configuration { get; private set; }

    public async Task InitializeAsync()
    {
        // Load test configuration
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("test-settings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        Configuration = configBuilder.Build();

        // Create database service
        DatabaseService = new DatabaseService(Configuration);

        // Initialize database
        await DatabaseService.InitializeDatabaseAsync();
        // Skip schema update for tests - use manual table creation
        // await DatabaseService.UpdateDatabaseSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up test data
        await CleanupTestDataAsync();
    }

    protected async Task InitializeTestDatabaseAsync()
    {
        if (DatabaseService == null)
            throw new InvalidOperationException("DatabaseService not initialized. Call InitializeAsync first.");

        // Clean up any existing test data
        await CleanupTestDataAsync();
    }

    protected async Task CleanupTestDataAsync()
    {
        if (DatabaseService == null) return;

        using var connection = await DatabaseService.GetConnectionAsync();
        
        // Clean up in reverse order of dependencies
        var cleanupCommands = new[]
        {
            "DELETE FROM AnalysisResults",
            "DELETE FROM FileUploads", 
            "DELETE FROM Users",
            "DELETE FROM Conversations",
            "DELETE FROM Quizzes"
        };

        foreach (var command in cleanupCommands)
        {
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = command;
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }
}
