namespace MediatorX;

/// <summary>
/// Represents a validation error
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Initializes a new instance of the ValidationError class
    /// </summary>
    /// <param name="propertyName">The name of the property that failed validation</param>
    /// <param name="errorMessage">The error message</param>
    public ValidationError(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets the name of the property that failed validation
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the error message
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Returns a string representation of the validation error
    /// </summary>
    /// <returns>A string representation of the validation error</returns>
    public override string ToString() => $"{PropertyName}: {ErrorMessage}";
}