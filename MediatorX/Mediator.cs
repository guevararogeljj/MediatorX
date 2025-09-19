using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediatorX;

/// <summary>
/// Implementation of IMediator with built-in validation
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the Mediator class
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) 
        where TRequest : IRequest
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Perform validation first
        var validationResult = await Validate(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult);

        // Get the handler
        var handler = GetService<IRequestHandler<TRequest>>();
        if (handler == null)
            throw new InvalidOperationException($"No handler found for request type {typeof(TRequest).Name}");

        // Execute the handler
        await handler.Handle(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Perform validation first
        var validationResult = await Validate(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult);

        // Get the handler using reflection to handle the generic types
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
            throw new InvalidOperationException($"No handler found for request type {requestType.Name}");

        // Execute the handler
        var handleMethod = handlerType.GetMethod("Handle");
        var result = handleMethod!.Invoke(handler, new object[] { request, cancellationToken });
        
        if (result is Task<TResponse> taskResult)
            return await taskResult;
        
        throw new InvalidOperationException($"Handler for {requestType.Name} did not return expected type");
    }

    /// <inheritdoc />
    public async Task<ValidationResult> Validate<TRequest>(TRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return ValidationResult.Failure(new ValidationError("Request", "Request cannot be null"));

        // Get all validators for this request type
        var validators = GetServices<IValidator<TRequest>>();
        
        if (!validators.Any())
            return ValidationResult.Success(); // No validators means valid

        var validationTasks = validators.Select(v => v.ValidateAsync(request, cancellationToken));
        var validationResults = await Task.WhenAll(validationTasks);

        var allErrors = validationResults.SelectMany(vr => vr.Errors).ToList();
        
        return allErrors.Any() ? new ValidationResult(allErrors) : ValidationResult.Success();
    }

    private T? GetService<T>() => (T?)_serviceProvider.GetService(typeof(T));
    
    private IEnumerable<T> GetServices<T>() => 
        _serviceProvider.GetService(typeof(IEnumerable<T>)) as IEnumerable<T> ?? Enumerable.Empty<T>();
}