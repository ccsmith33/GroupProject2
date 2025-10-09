using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using StudentStudyAI.Controllers;
using StudentStudyAI.Services;
using StudentStudyAI.Models;
using Xunit;

namespace StudentStudyAI.Tests.Controllers;

public class UserControllerTests
{
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController();
    }

    [Fact]
    public void UserController_ShouldBeCreated()
    {
        // Arrange & Act
        var controller = new UserController();

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void UserController_ShouldHaveAuthorizeAttribute()
    {
        // Arrange
        var controllerType = typeof(UserController);

        // Act
        var authorizeAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .FirstOrDefault() as AuthorizeAttribute;

        // Assert
        Assert.NotNull(authorizeAttribute);
    }

    [Fact]
    public void UserController_ShouldHaveApiControllerAttribute()
    {
        // Arrange
        var controllerType = typeof(UserController);

        // Act
        var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), false)
            .FirstOrDefault() as ApiControllerAttribute;

        // Assert
        Assert.NotNull(apiControllerAttribute);
    }

    [Fact]
    public void UserController_ShouldHaveCorrectRoute()
    {
        // Arrange
        var controllerType = typeof(UserController);

        // Act
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        Assert.NotNull(routeAttribute);
        Assert.Equal("api/[controller]", routeAttribute.Template);
    }
}
