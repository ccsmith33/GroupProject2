using StudentStudyAI.Models;
using StudentStudyAI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database service
builder.Services.AddScoped<DatabaseService>();

// Add CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000", "file://")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => "API is running");

// Database initialization endpoint
app.MapPost("/init-db", async (DatabaseService dbService) =>
{
    try
    {
        await dbService.InitializeDatabaseAsync();
        return Results.Ok(new { 
            message = "Database initialized successfully",
            details = "Created StudentStudyAI database with all required tables",
            tables = new[] { "Users", "FileUploads", "AnalysisResults", "StudySessions", "ChunkedFiles" }
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Database Setup Error"
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: $"Database initialization failed: {ex.Message}",
            statusCode: 500,
            title: "Unexpected Error"
        );
    }
});

app.Run();
