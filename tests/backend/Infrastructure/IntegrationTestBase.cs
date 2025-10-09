using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudentStudyAI.Services;
using System.Net.Http;
using Xunit;

namespace StudentStudyAI.Tests.Infrastructure;

public class IntegrationTestBase : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected HttpClient Client { get; private set; } = null!;
    protected IServiceScope Scope { get; private set; } = null!;
    protected DatabaseService DatabaseService { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("test-settings.json", optional: false, reloadOnChange: true);
        });

        builder.ConfigureServices(services =>
        {
            // Override services for testing
            services.AddScoped<DatabaseService>();
            services.AddScoped<JwtService>();
            services.AddScoped<FileStorageService>();
            services.AddScoped<UserService>();
            services.AddScoped<OpenAIService>();
            services.AddScoped<AnalysisService>();
            services.AddScoped<AIResponseParser>();
            services.AddScoped<ValidationService>();
            services.AddScoped<FileProcessingService>();
            services.AddScoped<ContextService>();
            services.AddScoped<CachedAnalysisService>();

            // Add processors
            services.AddScoped<StudentStudyAI.Services.Processors.IPdfProcessor, StudentStudyAI.Services.Processors.PdfProcessor>();
            services.AddScoped<StudentStudyAI.Services.Processors.IWordProcessor, StudentStudyAI.Services.Processors.WordProcessor>();
            services.AddScoped<StudentStudyAI.Services.Processors.IImageProcessor, StudentStudyAI.Services.Processors.ImageProcessor>();
            services.AddScoped<StudentStudyAI.Services.Processors.IMediaProcessor, StudentStudyAI.Services.Processors.MediaProcessor>();

            // Add memory cache
            services.AddMemoryCache();

            // Add HTTP client for OpenAI
            services.AddHttpClient<OpenAIService>();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        Client = CreateClient();
        Scope = Services.CreateScope();
        DatabaseService = Scope.ServiceProvider.GetRequiredService<DatabaseService>();

        // Initialize test database
        await DatabaseService.InitializeDatabaseAsync();
        // Skip schema update for tests - use manual table creation
        // await DatabaseService.UpdateDatabaseSchemaAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        // Clean up test data
        try
        {
            await CleanupTestDataAsync();
        }
        catch
        {
            // Ignore cleanup errors
        }

        Scope?.Dispose();
        Client?.Dispose();
        await base.DisposeAsync();
    }

    // Implement IAsyncLifetime.DisposeAsync
    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    protected async Task CleanupTestDataAsync()
    {
        using var connection = await DatabaseService.GetConnectionAsync();
        
        // Clean up in reverse dependency order
        var cleanupQueries = new[]
        {
            "DELETE FROM QuizAttempts",
            "DELETE FROM Quizzes", 
            "DELETE FROM FileStudyGuideLinks",
            "DELETE FROM StudyGuides",
            "DELETE FROM Conversations",
            "DELETE FROM UserSessions",
            "DELETE FROM AnalysisResults",
            "DELETE FROM ChunkedFiles",
            "DELETE FROM FileUploads",
            "DELETE FROM Users"
        };

        foreach (var query in cleanupQueries)
        {
            try
            {
                using var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1146) // Table doesn't exist
            {
                // Ignore table not found errors during cleanup
            }
        }
    }

    protected async Task<string> CreateTestUserAsync(string email, string password = "TestPassword123!")
    {
        var userService = Scope.ServiceProvider.GetRequiredService<UserService>();
        var registerRequest = new StudentStudyAI.Models.RegisterRequest
        {
            Username = "testuser",
            Email = email,
            Password = password
        };

        await userService.CreateUserAsync(registerRequest);
        var user = await userService.GetUserByEmailAsync(email);
        return user?.Id.ToString() ?? "0";
    }

    protected async Task<string> GetAuthTokenAsync(string email, string password = "TestPassword123!")
    {
        var userService = Scope.ServiceProvider.GetRequiredService<UserService>();
        var jwtService = Scope.ServiceProvider.GetRequiredService<JwtService>();
        
        var user = await userService.GetUserByEmailAsync(email);
        if (user != null)
        {
            return jwtService.GenerateToken(user);
        }

        return "";
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string endpoint, T data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await Client.PostAsync(endpoint, content);
    }

    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(string endpoint, T data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await Client.PutAsync(endpoint, content);
    }

    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
