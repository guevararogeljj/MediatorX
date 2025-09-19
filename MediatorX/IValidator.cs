using System.Threading;
using System.Threading.Tasks;

namespace MediatorX;

/// <summary>
/// Defines a validator for a request
/// </summary>
/// <typeparam name="TRequest">The type of request to validate</typeparam>
public interface IValidator<in TRequest>
{
    /// <summary>
    /// Validates the specified request
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the validation operation</returns>
    Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}