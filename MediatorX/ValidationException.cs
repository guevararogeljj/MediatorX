using System;

namespace MediatorX;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    /// <param name="validationResult">The validation result containing the errors</param>
    public ValidationException(ValidationResult validationResult)
        : base($"Validation failed with {validationResult.Errors.Count} error(s)")
    {
        ValidationResult = validationResult;
    }

    /// <summary>
    /// Gets the validation result containing the errors
    /// </summary>
    public ValidationResult ValidationResult { get; }
}