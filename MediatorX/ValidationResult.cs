using System.Collections.Generic;
using System.Linq;

namespace MediatorX;

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Initializes a new instance of the ValidationResult class
    /// </summary>
    /// <param name="errors">The validation errors</param>
    public ValidationResult(IEnumerable<ValidationError> errors)
    {
        Errors = errors?.ToList() ?? new List<ValidationError>();
    }

    /// <summary>
    /// Gets a value indicating whether the validation was successful
    /// </summary>
    public bool IsValid => !Errors.Any();

    /// <summary>
    /// Gets the list of validation errors
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    /// <returns>A successful validation result</returns>
    public static ValidationResult Success() => new(Enumerable.Empty<ValidationError>());

    /// <summary>
    /// Creates a failed validation result with the specified errors
    /// </summary>
    /// <param name="errors">The validation errors</param>
    /// <returns>A failed validation result</returns>
    public static ValidationResult Failure(params ValidationError[] errors) => new(errors);
}