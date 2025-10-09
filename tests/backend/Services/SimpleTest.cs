using Xunit;

namespace StudentStudyAI.Tests.Services;

public class SimpleTest
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Arrange
        var expected = 2;
        
        // Act
        var actual = 1 + 1;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}
