using System.Threading;
using System.Threading.Tasks;

namespace MediatorX;

/// <summary>
/// Handler for a request without a response
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
public interface IRequestHandler<in TRequest> 
    where TRequest : IRequest
{
    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task Handle(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for a request with a response
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation with the response</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}