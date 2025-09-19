using Microsoft.Extensions.DependencyInjection;

public interface IMediatorX
{
    /// <summary>
    /// Sends a request to its corresponding handler.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request object.</param>
    /// <returns>A task that represents the asynchronous operation, with the handler's response.</returns>
    Task<TResponse> Send<TResponse>(IRequestX<TResponse> request);
}

/// <summary>
/// Represents a request that returns a value.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IRequestX<out TResponse>
{
}

/// <summary>
/// Defines a handler for a request.
/// </summary>
/// <typeparam name="TCommand">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IRequestXHandler<in TCommand, TResponse> where TCommand : IRequestX<TResponse>
{
    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="command">The request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    Task<TResponse> Handle(TCommand command);
}

/// <summary>
/// Mediator implementation.
/// </summary>
public class MediatorX : IMediatorX
{
    private readonly IServiceProvider _serviceProvider;

    public MediatorX(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequestX<TResponse> request)
    {
        var commandType = request.GetType();
        var handlerType = typeof(IRequestXHandler<,>).MakeGenericType(commandType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"Handler for request '{commandType.Name}' was not found.");
        }

        var handleMethod = handler.GetType().GetMethod("Handle");

        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handler for '{commandType.Name}' does not have a 'Handle' method.");
        }

        return (Task<TResponse>)handleMethod.Invoke(handler, new object[] { request });
    }
}

// En MediatorX/MediatorX.cs

public interface IRequestXValidator<T>
{
    List<string> Validate(T request);
}

public class MediatorXWithValidation : IMediatorX
{
    private readonly IServiceProvider _provider;
    private readonly IMediatorX _inner;

    public MediatorXWithValidation(IServiceProvider provider, IMediatorX inner)
    {
        _provider = provider;
        _inner = inner;
    }

    public async Task<TResponse> Send<TResponse>(IRequestX<TResponse> request)
    {
        var validatorType = typeof(IRequestXValidator<>).MakeGenericType(request.GetType());
        var validator = _provider.GetService(validatorType) as dynamic;
        if (validator != null)
        {
            List<string> errors = validator.Validate((dynamic)request);
            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }
        return await _inner.Send<TResponse>(request);
    }
}

public class ValidationException : Exception
{
    public List<string> Errors { get; }
    public ValidationException(List<string> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}