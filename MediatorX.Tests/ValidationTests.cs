using Xunit;

namespace MediatorX.Tests;

public class ValidationResultTests
{
    [Fact]
    public void Success_ReturnsValidResult()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_WithErrors_ReturnsInvalidResult()
    {
        // Arrange
        var error1 = new ValidationError("Property1", "Error 1");
        var error2 = new ValidationError("Property2", "Error 2");

        // Act
        var result = ValidationResult.Failure(error1, error2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(error1, result.Errors);
        Assert.Contains(error2, result.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyList()
    {
        // Act
        var result = new ValidationResult(null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var propertyName = "TestProperty";
        var errorMessage = "Test error message";

        // Act
        var error = new ValidationError(propertyName, errorMessage);

        // Assert
        Assert.Equal(propertyName, error.PropertyName);
        Assert.Equal(errorMessage, error.ErrorMessage);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var error = new ValidationError("TestProperty", "Test error");

        // Act
        var result = error.ToString();

        // Assert
        Assert.Equal("TestProperty: Test error", result);
    }
}

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_SetsValidationResult()
    {
        // Arrange
        var validationResult = ValidationResult.Failure(
            new ValidationError("Test", "Error"));

        // Act
        var exception = new ValidationException(validationResult);

        // Assert
        Assert.Equal(validationResult, exception.ValidationResult);
        Assert.Contains("Validation failed with 1 error(s)", exception.Message);
    }
}