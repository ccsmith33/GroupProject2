using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace StudentStudyAI.Tests.Integration;

[Collection("Integration")]
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly IDatabaseService _databaseService;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Add test database configuration
                services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=StudentStudyAI_Test;Uid=test;Pwd=test;",
                        ["Jwt:SecretKey"] = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                        ["Jwt:Issuer"] = "StudentStudyAI",
                        ["Jwt:Audience"] = "StudentStudyAI",
                        ["Jwt:ExpiryMinutes"] = "60",
                        ["FileStorage:Path"] = "test-uploads",
                        ["OpenAI:UseMock"] = "true"
                    })
                    .Build());
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _databaseService = _scope.ServiceProvider.GetRequiredService<IDatabaseService>();
    }

    [Fact]
    public void ApiIntegrationTests_ShouldBeCreated()
    {
        // Arrange & Act
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public async Task CompleteUserFlow_ShouldWorkEndToEnd()
    {
        // Step 1: Register a new user
        var registerRequest = new RegisterRequest
        {
            Username = "integrationtest",
            Email = "integration@test.com",
            Password = "password123"
        };

        var registerJson = JsonSerializer.Serialize(registerRequest);
        var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        registerResponse.EnsureSuccessStatusCode();

        var registerResult = await registerResponse.Content.ReadAsStringAsync();
        var registerData = JsonSerializer.Deserialize<JsonElement>(registerResult);
        var token = registerData.GetProperty("token").GetString();
        var userId = registerData.GetProperty("userId").GetInt32();

        Assert.NotNull(token);
        Assert.True(userId > 0);

        // Step 2: Login with the registered user
        var loginRequest = new LoginRequest
        {
            Email = "integration@test.com",
            Password = "password123"
        };

        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginResult);
        var loginToken = loginData.GetProperty("token").GetString();

        Assert.NotNull(loginToken);
        Assert.Equal(token, loginToken);

        // Step 3: Get user profile
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var profileResponse = await _client.GetAsync($"/api/user/profile/{userId}");
        profileResponse.EnsureSuccessStatusCode();

        var profileResult = await profileResponse.Content.ReadAsStringAsync();
        var profileData = JsonSerializer.Deserialize<JsonElement>(profileResult);
        var username = profileData.GetProperty("username").GetString();

        Assert.Equal("integrationtest", username);

        // Step 4: Logout
        var logoutResponse = await _client.PostAsync($"/api/auth/logout/{userId}?token={token}", null);
        logoutResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CompleteFileAnalysisFlow_ShouldWorkEndToEnd()
    {
        // Step 1: Register and login
        var (token, userId) = await RegisterAndLoginUser();

        // Step 2: Upload a test file
        var fileContent = "This is a test document about mathematics and algebra concepts.";
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);
        var fileStream = new MemoryStream(fileBytes);

        var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(fileStream), "file", "test.txt");
        formData.Add(new StringContent(userId.ToString()), "userId");
        formData.Add(new StringContent("Mathematics"), "subject");
        formData.Add(new StringContent("Intermediate"), "studentLevel");

        var uploadResponse = await _client.PostAsync("/api/files/upload", formData);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
        var uploadData = JsonSerializer.Deserialize<JsonElement>(uploadResult);
        var fileId = uploadData.GetProperty("fileId").GetInt32();

        Assert.True(fileId > 0);

        // Step 3: Check file status
        var statusResponse = await _client.GetAsync($"/api/files/{fileId}/status");
        statusResponse.EnsureSuccessStatusCode();

        var statusResult = await statusResponse.Content.ReadAsStringAsync();
        var statusData = JsonSerializer.Deserialize<JsonElement>(statusResult);
        var status = statusData.GetProperty("status").GetString();

        Assert.NotNull(status);

        // Step 4: Analyze the file
        var analysisResponse = await _client.PostAsync($"/api/analysis/analyze/{fileId}?userId={userId}", null);
        analysisResponse.EnsureSuccessStatusCode();

        var analysisResult = await analysisResponse.Content.ReadAsStringAsync();
        var analysisData = JsonSerializer.Deserialize<JsonElement>(analysisResult);
        var analysisId = analysisData.GetProperty("analysisId").GetInt32();

        Assert.True(analysisId > 0);

        // Step 5: Generate study guide
        var studyGuideResponse = await _client.PostAsync($"/api/analysis/study-guide/{analysisId}?userId={userId}", null);
        studyGuideResponse.EnsureSuccessStatusCode();

        var studyGuideResult = await studyGuideResponse.Content.ReadAsStringAsync();
        var studyGuideData = JsonSerializer.Deserialize<JsonElement>(studyGuideResult);
        var studyGuideId = studyGuideData.GetProperty("studyGuideId").GetInt32();

        Assert.True(studyGuideId > 0);

        // Step 6: Generate quiz
        var quizResponse = await _client.PostAsync($"/api/analysis/quiz/{analysisId}?userId={userId}", null);
        quizResponse.EnsureSuccessStatusCode();

        var quizResult = await quizResponse.Content.ReadAsStringAsync();
        var quizData = JsonSerializer.Deserialize<JsonElement>(quizResult);
        var quizId = quizData.GetProperty("quizId").GetInt32();

        Assert.True(quizId > 0);

        // Step 7: Get file list
        var filesResponse = await _client.GetAsync($"/api/files/list?userId={userId}");
        filesResponse.EnsureSuccessStatusCode();

        var filesResult = await filesResponse.Content.ReadAsStringAsync();
        var filesData = JsonSerializer.Deserialize<JsonElement[]>(filesResult);

        Assert.True(filesData.Length > 0);
        Assert.Equal("test.txt", filesData[0].GetProperty("fileName").GetString());

        // Step 8: Delete the file
        var deleteResponse = await _client.DeleteAsync($"/api/files/{fileId}");
        deleteResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CompleteStudyGuideFlow_ShouldWorkEndToEnd()
    {
        // Step 1: Register and login
        var (token, userId) = await RegisterAndLoginUser();

        // Step 2: Upload a file
        var (fileId, analysisId) = await UploadAndAnalyzeFile(userId);

        // Step 3: Generate study guide
        var studyGuideRequest = new StudyGuideRequest
        {
            UserPrompt = "Create a study guide for algebra and calculus"
        };

        var studyGuideJson = JsonSerializer.Serialize(studyGuideRequest);
        var studyGuideContent = new StringContent(studyGuideJson, Encoding.UTF8, "application/json");

        var studyGuideResponse = await _client.PostAsync($"/api/analysis/study-guide/{analysisId}?userId={userId}", studyGuideContent);
        studyGuideResponse.EnsureSuccessStatusCode();

        var studyGuideResult = await studyGuideResponse.Content.ReadAsStringAsync();
        var studyGuideData = JsonSerializer.Deserialize<JsonElement>(studyGuideResult);
        var studyGuideId = studyGuideData.GetProperty("studyGuideId").GetInt32();

        Assert.True(studyGuideId > 0);

        // Step 4: Get study guide
        var getStudyGuideResponse = await _client.GetAsync($"/api/analysis/study-guide/{studyGuideId}");
        getStudyGuideResponse.EnsureSuccessStatusCode();

        var getStudyGuideResult = await getStudyGuideResponse.Content.ReadAsStringAsync();
        var getStudyGuideData = JsonSerializer.Deserialize<JsonElement>(getStudyGuideResult);
        var title = getStudyGuideData.GetProperty("title").GetString();

        Assert.NotNull(title);
        Assert.NotEmpty(title);

        // Cleanup
        await _client.DeleteAsync($"/api/files/{fileId}");
    }

    [Fact]
    public async Task AuthenticationFlow_ShouldWorkEndToEnd()
    {
        // Step 1: Try to access protected endpoint without token
        var protectedResponse = await _client.GetAsync("/api/user/profile/1");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, protectedResponse.StatusCode);

        // Step 2: Register a user
        var (token, userId) = await RegisterAndLoginUser();

        // Step 3: Access protected endpoint with token
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var profileResponse = await _client.GetAsync($"/api/user/profile/{userId}");
        profileResponse.EnsureSuccessStatusCode();

        // Step 4: Refresh token
        var refreshResponse = await _client.PostAsync($"/api/auth/refresh/{userId}", null);
        refreshResponse.EnsureSuccessStatusCode();

        var refreshResult = await refreshResponse.Content.ReadAsStringAsync();
        var refreshData = JsonSerializer.Deserialize<JsonElement>(refreshResult);
        var newToken = refreshData.GetProperty("token").GetString();

        Assert.NotNull(newToken);
        Assert.NotEqual(token, newToken);

        // Step 5: Use new token
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
        var profileResponse2 = await _client.GetAsync($"/api/user/profile/{userId}");
        profileResponse2.EnsureSuccessStatusCode();

        // Step 6: Logout
        var logoutResponse = await _client.PostAsync($"/api/auth/logout/{userId}?token={newToken}", null);
        logoutResponse.EnsureSuccessStatusCode();

        // Step 7: Try to use invalidated token
        var invalidResponse = await _client.GetAsync($"/api/user/profile/{userId}");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, invalidResponse.StatusCode);
    }

    private async Task<(string token, int userId)> RegisterAndLoginUser()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        var registerJson = JsonSerializer.Serialize(registerRequest);
        var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        registerResponse.EnsureSuccessStatusCode();

        var registerResult = await registerResponse.Content.ReadAsStringAsync();
        var registerData = JsonSerializer.Deserialize<JsonElement>(registerResult);
        var token = registerData.GetProperty("token").GetString()!;
        var userId = registerData.GetProperty("userId").GetInt32();

        return (token, userId);
    }

    private async Task<(int fileId, int analysisId)> UploadAndAnalyzeFile(int userId)
    {
        // Upload file
        var fileContent = "This is a test document about mathematics and algebra concepts.";
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);
        var fileStream = new MemoryStream(fileBytes);

        var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(fileStream), "file", "test.txt");
        formData.Add(new StringContent(userId.ToString()), "userId");
        formData.Add(new StringContent("Mathematics"), "subject");
        formData.Add(new StringContent("Intermediate"), "studentLevel");

        var uploadResponse = await _client.PostAsync("/api/files/upload", formData);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
        var uploadData = JsonSerializer.Deserialize<JsonElement>(uploadResult);
        var fileId = uploadData.GetProperty("fileId").GetInt32();

        // Analyze file
        var analysisResponse = await _client.PostAsync($"/api/analysis/analyze/{fileId}?userId={userId}", null);
        analysisResponse.EnsureSuccessStatusCode();

        var analysisResult = await analysisResponse.Content.ReadAsStringAsync();
        var analysisData = JsonSerializer.Deserialize<JsonElement>(analysisResult);
        var analysisId = analysisData.GetProperty("analysisId").GetInt32();

        return (fileId, analysisId);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _scope?.Dispose();
        _factory?.Dispose();
    }
}
