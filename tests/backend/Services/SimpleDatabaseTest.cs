using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using StudentStudyAI.Tests.Infrastructure;
using System.Data;
using MySql.Data.MySqlClient;
using Xunit;

namespace StudentStudyAI.Tests.Services;

public class SimpleDatabaseTest : DatabaseTestBase
{
    private readonly Mock<ILogger<DatabaseService>> _mockLogger;

    public SimpleDatabaseTest()
    {
        _mockLogger = new Mock<ILogger<DatabaseService>>();
    }

    [Fact]
    public async Task GetConnectionAsync_ShouldReturnValidConnection()
    {
        // Act
        using var connection = await DatabaseService!.GetConnectionAsync();

        // Assert
        connection.Should().NotBeNull();
        connection.State.Should().Be(ConnectionState.Open);
    }

    [Fact]
    public async Task InitializeDatabaseAsync_ShouldCreateTables()
    {
        // Act
        await DatabaseService!.InitializeDatabaseAsync();

        // Assert
        var usersTableExists = await TableExistsAsync("Users");
        var fileUploadsTableExists = await TableExistsAsync("FileUploads");
        var analysisResultsTableExists = await TableExistsAsync("AnalysisResults");

        usersTableExists.Should().BeTrue();
        fileUploadsTableExists.Should().BeTrue();
        analysisResultsTableExists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUserSuccessfully()
    {
        // Arrange
        await InitializeTestDatabaseAsync();
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        // Act
        await DatabaseService!.CreateUserAsync(user);

        // Assert
        var exists = await UserExistsAsync(user.Email);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldReturnUser()
    {
        // Arrange
        await InitializeTestDatabaseAsync();
        var email = "test@example.com";
        var user = new User
        {
            Username = "testuser",
            Email = email,
            PasswordHash = "hashedpassword",
            IsActive = true
        };
        await DatabaseService!.CreateUserAsync(user);

        // Act
        var retrievedUser = await DatabaseService.GetUserByEmailAsync(email);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be(email);
    }

    // Helper methods
    private async Task<bool> UserExistsAsync(string email)
    {
        using var connection = await DatabaseService!.GetConnectionAsync();
        var command = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @email", connection);
        command.Parameters.AddWithValue("@email", email);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private async Task<bool> TableExistsAsync(string tableName)
    {
        using var connection = await DatabaseService!.GetConnectionAsync();
        var command = new MySqlCommand("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @tableName", connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }
}
