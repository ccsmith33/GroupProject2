using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace api.Tests
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetProjects_ReturnsSuccessStatusCode()
        {
            // Arrange
            var request = "/api/projects";

            // Act
            var response = await _client.GetAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", 
                response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task CreateProject_WithValidData_ReturnsCreated()
        {
            // Arrange
            var request = new
            {
                Name = "Test Project",
                Description = "Test Description"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/projects", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateProject_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var request = new
            {
                Name = "",
                Description = "Test Description"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/projects", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
