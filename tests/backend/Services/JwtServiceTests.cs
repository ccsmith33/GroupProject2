using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Models;
using StudentStudyAI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace StudentStudyAI.Tests.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<JwtService>> _mockLogger;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<JwtService>>();
        
        // Setup default configuration
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        jwtSection.Setup(x => x["Issuer"]).Returns("StudentStudyAI");
        jwtSection.Setup(x => x["Audience"]).Returns("StudentStudyAI");
        jwtSection.Setup(x => x["ExpiryMinutes"]).Returns("60");

        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);
        
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public void GenerateToken_ShouldCreateValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify it's a valid JWT format
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Equal("StudentStudyAI", jsonToken.Issuer);
        Assert.Equal("StudentStudyAI", jsonToken.Audiences.First());
        Assert.Equal("test@example.com", jsonToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal("1", jsonToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }

    [Fact]
    public void ValidateToken_ShouldReturnTrueForValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var claimsPrincipal = _jwtService.ValidateToken(token);

        // Assert
        Assert.NotNull(claimsPrincipal);
        Assert.True(claimsPrincipal.Identity!.IsAuthenticated);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNullForExpiredToken()
    {
        // Arrange
        var expiredConfig = new Mock<IConfiguration>();
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        jwtSection.Setup(x => x["Issuer"]).Returns("StudentStudyAI");
        jwtSection.Setup(x => x["Audience"]).Returns("StudentStudyAI");
        jwtSection.Setup(x => x["ExpiryMinutes"]).Returns("-1"); // Expired immediately

        expiredConfig.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);
        
        var expiredJwtService = new JwtService(expiredConfig.Object, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };
        var token = expiredJwtService.GenerateToken(user);

        // Wait a moment to ensure token is expired
        Thread.Sleep(100);

        // Act
        var claimsPrincipal = _jwtService.ValidateToken(token);

        // Assert
        Assert.Null(claimsPrincipal);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNullForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var claimsPrincipal = _jwtService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(claimsPrincipal);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNullForEmptyToken()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var claimsPrincipal = _jwtService.ValidateToken(emptyToken);

        // Assert
        Assert.Null(claimsPrincipal);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNullForNullToken()
    {
        // Arrange
        string? nullToken = null;

        // Act
        var claimsPrincipal = _jwtService.ValidateToken(nullToken!);

        // Assert
        Assert.Null(claimsPrincipal);
    }

    [Fact]
    public void HashPassword_ShouldCreateSecureHash()
    {
        // Arrange
        var password = "testpassword123";

        // Act
        var hash = _jwtService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash);
        Assert.True(hash.Length > 50); // BCrypt hashes are typically 60 characters
    }

    [Fact]
    public void HashPassword_ShouldCreateDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "testpassword123";

        // Act
        var hash1 = _jwtService.HashPassword(password);
        var hash2 = _jwtService.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // BCrypt should generate different salts
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var password = "testpassword123";
        var hash = _jwtService.HashPassword(password);

        // Act
        var isValid = _jwtService.VerifyPassword(password, hash);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var password = "testpassword123";
        var wrongPassword = "wrongpassword";
        var hash = _jwtService.HashPassword(password);

        // Act
        var isValid = _jwtService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForEmptyPassword()
    {
        // Arrange
        var password = "testpassword123";
        var emptyPassword = "";
        var hash = _jwtService.HashPassword(password);

        // Act
        var isValid = _jwtService.VerifyPassword(emptyPassword, hash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForNullPassword()
    {
        // Arrange
        var password = "testpassword123";
        string? nullPassword = null;
        var hash = _jwtService.HashPassword(password);

        // Act
        var isValid = _jwtService.VerifyPassword(nullPassword!, hash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeAllRequiredClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "testuser");
        Assert.Contains(jsonToken.Claims, c => c.Type == "jti");
        Assert.Contains(jsonToken.Claims, c => c.Type == "iat");
        Assert.Contains(jsonToken.Claims, c => c.Type == "exp");
    }

    [Fact]
    public void GenerateToken_ShouldHandleUserWithNullUsername()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "",
            Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "");
    }
}