using Xunit;

namespace StudentStudyAI.Tests.Services;

public class MinimalTest
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

    [Fact]
    public void StringTest_ShouldPass()
    {
        // Arrange
        var expected = "Hello World";

        // Act
        var actual = "Hello " + "World";

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ListTest_ShouldPass()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };

        // Act & Assert
        Assert.Equal(3, list.Count);
        Assert.Contains("item2", list);
        Assert.DoesNotContain("item4", list);
    }
}
