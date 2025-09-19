namespace MediatorX;

/// <summary>
/// Marker interface to represent a request
/// </summary>
public interface IRequest
{
}

/// <summary>
/// Marker interface to represent a request with a response
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IRequest<out TResponse> : IRequest
{
}