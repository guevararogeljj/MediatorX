using System.Threading;
using System.Threading.Tasks;

namespace MediatorX;

/// <summary>
/// Mediator interface with built-in validation support
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request without a response with validation
    /// </summary>
    /// <typeparam name="TRequest">The type of request</typeparam>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) 
        where TRequest : IRequest;

    /// <summary>
    /// Sends a request with a response with validation
    /// </summary>
    /// <typeparam name="TResponse">The type of response</typeparam>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation with the response</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a request without sending it
    /// </summary>
    /// <typeparam name="TRequest">The type of request</typeparam>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the validation operation</returns>
    Task<ValidationResult> Validate<TRequest>(TRequest request, CancellationToken cancellationToken = default);
}