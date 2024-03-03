using Beseler.Domain.Common;

namespace Beseler.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void IsSuccess_ReturnsTrue_WhenResultIsSuccess()
    {
        // Arrange
        Success success = new();

        // Act
        var result = (Result<Success, Exception>)success;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void IsFailure_ReturnsTrue_WhenResultIsFailure()
    {
        // Arrange
        Exception error = new();

        // Act
        var result = (Result<Success, Exception>)error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Match_ExecutesOnSuccess_WhenResultIsSuccess()
    {
        // Arrange
        Success success = new();

        // Act
        var result = (Result<Success, Exception>)success;

        // Assert
        result.Match(onSuccess: _ => true, onFailure: _ => false).Should().BeTrue();
    }

    [Fact]
    public void Match_ExecutesOnFailure_WhenResultIsFailure()
    {
        // Arrange
        Exception error = new();

        // Act
        var result = (Result<Success, Exception>)error;

        // Assert
        result.Match(onSuccess: _ => false, onFailure: _ => true).Should().BeTrue();
    }
}
