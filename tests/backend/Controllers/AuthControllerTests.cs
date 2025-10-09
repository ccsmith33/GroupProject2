using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using StudentStudyAI.Controllers;
using StudentStudyAI.Services;
using StudentStudyAI.Models;
using Xunit;

namespace StudentStudyAI.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserService> _mockUserService;
    private readonly Mock<JwtService> _mockJwtService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserService = new Mock<UserService>(
            Mock.Of<DatabaseService>(),
            Mock.Of<JwtService>(),
            Mock.Of<ILogger<UserService>>());
        _mockJwtService = new Mock<JwtService>(
            Mock.Of<IConfiguration>(),
            Mock.Of<ILogger<JwtService>>());
        _mockLogger = new Mock<ILogger<AuthController>>();
        
        _controller = new AuthController(_mockUserService.Object, _mockJwtService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Register_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var user = new User
        {
            Id = 1,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = "hashed_password_123"
        };

        _mockUserService.Setup(x => x.CreateUserAsync(request))
            .ReturnsAsync(user);

        var token = "jwt_token_123";
        _mockJwtService.Setup(x => x.GenerateToken(user))
            .Returns(token);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(token, response.AccessToken);
        Assert.NotNull(response.User);
        Assert.Equal(user.Username, response.User.Username);
    }

    [Fact]
    public async Task Register_ShouldReturnErrorForDuplicateEmail()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _mockUserService.Setup(x => x.CreateUserAsync(request))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<dynamic>(badRequestResult.Value);
        Assert.Equal("Email already exists", response.error);
    }

    [Fact]
    public async Task Login_ShouldReturnTokenForValidCredentials()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_password_123"
        };

        _mockUserService.Setup(x => x.AuthenticateUserAsync(request.Email, request.Password))
            .ReturnsAsync(user);

        var token = "jwt_token_123";
        _mockJwtService.Setup(x => x.GenerateToken(user))
            .Returns(token);

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        _mockUserService.Setup(x => x.MapToUserInfo(user))
            .Returns(userInfo);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(token, response.AccessToken);
        Assert.NotNull(response.User);
        Assert.Equal(user.Username, response.User.Username);
    }

    [Fact]
    public async Task Login_ShouldReturnErrorForInvalidCredentials()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        _mockUserService.Setup(x => x.AuthenticateUserAsync(request.Email, request.Password))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<dynamic>(unauthorizedResult.Value);
        Assert.Equal("Invalid credentials", response.error);
    }

    [Fact]
    public async Task Login_ShouldReturnErrorForNonExistentUser()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUserService.Setup(x => x.AuthenticateUserAsync(request.Email, request.Password))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<dynamic>(unauthorizedResult.Value);
        Assert.Equal("Invalid credentials", response.error);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNotImplemented()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "refresh_token_123"
        };

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<dynamic>(badRequestResult.Value);
        Assert.Equal("Not implemented", response.error);
    }

    [Fact]
    public async Task Register_ShouldReturnErrorForInvalidInput()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "",
            Email = "invalid-email",
            Password = "123",
            ConfirmPassword = "123"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<dynamic>(badRequestResult.Value);
        Assert.Equal("Validation failed", response.error);
    }

    [Fact]
    public async Task Login_ShouldReturnErrorForInvalidInput()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "",
            Password = ""
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<dynamic>(badRequestResult.Value);
        Assert.Equal("Validation failed", response.error);
    }
}
