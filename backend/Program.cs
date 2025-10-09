using StudentStudyAI.Models;
using StudentStudyAI.Services;
using Serilog;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/app-.txt", rollingInterval: Serilog.RollingInterval.Day, retainedFileCountLimit: 7);
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Student Study AI API",
        Version = "v1",
        Description = "AI-powered study platform with file processing and analysis capabilities",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Student Study AI Team",
            Email = "support@studentstudyai.com"
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add database service
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Add file processing services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
builder.Services.AddScoped<StudentStudyAI.Services.Processors.IPdfProcessor, StudentStudyAI.Services.Processors.PdfProcessor>();
builder.Services.AddScoped<StudentStudyAI.Services.Processors.IWordProcessor, StudentStudyAI.Services.Processors.WordProcessor>();
builder.Services.AddScoped<StudentStudyAI.Services.Processors.IPowerPointProcessor, StudentStudyAI.Services.Processors.PowerPointProcessor>();
builder.Services.AddScoped<StudentStudyAI.Services.Processors.IImageProcessor, StudentStudyAI.Services.Processors.ImageProcessor>();
builder.Services.AddScoped<StudentStudyAI.Services.Processors.IMediaProcessor, StudentStudyAI.Services.Processors.MediaProcessor>();

// Add new services for Phase 1 & 2
builder.Services.AddScoped<ContentAnalysisService>();
builder.Services.AddScoped<SubjectGroupService>();

// Add Phase 3 services
builder.Services.AddScoped<KnowledgeTrackingService>();

// Add authentication services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UserService>();

// Add AI services
builder.Services.AddHttpClient<OpenAIService>();
builder.Services.AddScoped<OpenAIService>();
builder.Services.AddScoped<ContextService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<AIResponseParser>();
builder.Services.AddScoped<CachedAnalysisService>();
builder.Services.AddScoped<ValidationService>();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
    var issuer = jwtSettings["Issuer"] ?? "StudentStudyAI";
    var audience = jwtSettings["Audience"] ?? "StudentStudyAI";

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add caching
builder.Services.AddMemoryCache();

// Add background services
builder.Services.AddScoped<BackgroundJobService>();
builder.Services.AddHostedService<BackgroundJobService>();

// Add CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure URLs explicitly - HTTP only for testing
app.Urls.Add("http://localhost:5000");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
// app.UseHttpsRedirection(); // Disabled for testing

// Serve static files for testing
app.UseStaticFiles();

// Add global exception handling
app.UseMiddleware<StudentStudyAI.Middleware.GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => "API is running");

// MySQL connection test endpoint
app.MapGet("/test-mysql", async (IServiceProvider serviceProvider) =>
{
    try
    {
        // Test basic MySQL connection without database
        var connectionStrings = new[]
        {
            "Server=localhost;Uid=root;Pwd=test;Port=3306;SslMode=None;AllowPublicKeyRetrieval=true;",
            "Server=localhost;Uid=root;Pwd=;Port=3306;SslMode=None;AllowPublicKeyRetrieval=true;",
            "Server=localhost;Uid=root;Pwd=root;Port=3306;SslMode=None;AllowPublicKeyRetrieval=true;",
            "Server=localhost;Uid=root;Pwd=password;Port=3306;SslMode=None;AllowPublicKeyRetrieval=true;"
        };
        
        MySqlConnection? connection = null;
        string workingConnectionString = "";
        
        foreach (var connStr in connectionStrings)
        {
            try
            {
                connection = new MySqlConnection(connStr);
                await connection.OpenAsync();
                workingConnectionString = connStr;
                break;
            }
            catch
            {
                connection?.Dispose();
                connection = null;
                continue;
            }
        }
        
        if (connection == null)
        {
            return Results.Problem(
                detail: "Could not connect with any of the tested connection strings. Please check:\n1. MySQL is running\n2. Root password is correct\n3. User has proper permissions",
                statusCode: 500,
                title: "MySQL Connection Failed"
            );
        }
        
        try
        {
            using var command = new MySqlCommand("SELECT VERSION() as version, USER() as user, DATABASE() as current_database", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            string version = "";
            string user = "";
            string database = "";
            
            if (await reader.ReadAsync())
            {
                version = reader.GetString(0);
                user = reader.GetString(1);
                database = reader.IsDBNull(2) ? "NULL" : reader.GetString(2);
            }
            
            return Results.Ok(new { 
                message = "MySQL connection successful",
                serverVersion = version,
                user = user,
                currentDatabase = database,
                workingConnectionString = workingConnectionString,
                note = "Update appsettings.json with the working connection string"
            });
        }
        finally
        {
            connection?.Dispose();
        }
    }
    catch (MySqlException ex)
    {
        return Results.Problem(
            detail: $"MySQL Error: {ex.Message} (Error Code: {ex.Number})\n\nTroubleshooting:\n1. Check if MySQL is running\n2. Verify username/password\n3. Check if port 3306 is open",
            statusCode: 500,
            title: "MySQL Connection Failed"
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: $"Connection test failed: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
            statusCode: 500,
            title: "Connection Test Error"
        );
    }
});

// Test page endpoint
app.MapGet("/test", () => Results.File("test-api.html", "text/html"));

// Database initialization endpoint
app.MapGet("/init-db", async (IServiceProvider serviceProvider) =>
{
    try
    {
                var dbService = serviceProvider.GetRequiredService<IDatabaseService>();
        await dbService.InitializeDatabaseAsync();
        return Results.Ok(new { 
            message = "Database initialized successfully",
            details = "Created StudentStudyAI database with all required tables",
            tables = new[] { "Users", "FileUploads", "AnalysisResults", "StudySessions", "ChunkedFiles" }
        });
    }
    catch (MySqlException ex)
    {
        return Results.Problem(
            detail: $"MySQL Error: {ex.Message} (Error Code: {ex.Number})",
            statusCode: 500,
            title: "Database Connection Error"
        );
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
            detail: $"Database initialization failed: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
            statusCode: 500,
            title: "Unexpected Error"
        );
    }
});

// Database update endpoint for new schema
app.MapPost("/update-db", async (IServiceProvider serviceProvider) =>
{
    try
    {
                var dbService = serviceProvider.GetRequiredService<IDatabaseService>();
        await dbService.UpdateDatabaseSchemaAsync();
        return Results.Ok(new { 
            message = "Database schema updated successfully",
            details = "Added new tables for context-aware study guide system",
            newTables = new[] { "StudyGuides", "Conversations", "UserSessions", "Quizzes", "QuizAttempts", "FileStudyGuideLinks" }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: $"Database update failed: {ex.Message}",
            statusCode: 500,
            title: "Database Update Error"
        );
    }
});

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var urls = app.Urls;
logger.LogInformation("🚀 Student Study AI API started successfully!");
logger.LogInformation("📡 Listening on: {Urls}", string.Join(", ", urls));
logger.LogInformation("📚 Swagger UI: {SwaggerUrl}", "https://localhost:5001/swagger");
logger.LogInformation("🔍 Health Check: {HealthUrl}", "https://localhost:5001/health");
logger.LogInformation("Press Ctrl+C to shut down.");

app.Run();

// Make Program class accessible for testing
public partial class Program { }