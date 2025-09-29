using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;

// ============================================================================
// CRITICAL: This is the main API entry point - DO NOT modify without team discussion
// Contains: CORS configuration, database setup, API endpoints
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for frontend communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var corsOrigins = builder.Configuration.GetSection("ApiSettings:CorsOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000", "http://127.0.0.1:3000", "file://" };
        
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handling
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Initialize database
using (var connection = new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    connection.Open();
    
    // Create a simple example table
    var createTableCommand = connection.CreateCommand();
    createTableCommand.CommandText = @"
        CREATE TABLE IF NOT EXISTS Projects (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Description TEXT,
            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        )";
    createTableCommand.ExecuteNonQuery();
}

// Example API endpoint
app.MapGet("/api/projects", async () =>
{
    var projects = new List<object>();
    using (var connection = new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
    {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Projects ORDER BY CreatedAt DESC";
        
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                projects.Add(new
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Description = reader.GetString("Description"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }
        }
    }
    return Results.Ok(projects);
})
.WithName("GetProjects")
.WithOpenApi();

app.MapPost("/api/projects", async (CreateProjectRequest request) =>
{
    // Basic validation
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Project name is required");
    }

    using (var connection = new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
    {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Projects (Name, Description) VALUES (@name, @description)";
        command.Parameters.AddWithValue("@name", request.Name.Trim());
        command.Parameters.AddWithValue("@description", request.Description?.Trim() ?? "");
        command.ExecuteNonQuery();
    }
    return Results.Created("/api/projects", request);
})
.WithName("CreateProject")
.WithOpenApi();

app.Run();

public record CreateProjectRequest(
    [Required] string Name, 
    string? Description
);
