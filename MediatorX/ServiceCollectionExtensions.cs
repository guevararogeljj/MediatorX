using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace MediatorX;

/// <summary>
/// Extension methods for IServiceCollection to register MediatorX services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MediatorX services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers and validators</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMediatorX(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register the mediator
        services.AddScoped<IMediator, Mediator>();

        // If no assemblies provided, use the calling assembly
        if (!assemblies.Any())
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Register handlers and validators from assemblies
        foreach (var assembly in assemblies)
        {
            RegisterHandlersAndValidators(services, assembly);
        }

        return services;
    }

    private static void RegisterHandlersAndValidators(IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
            .ToList();

        // Register request handlers
        foreach (var type in types)
        {
            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToList();

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, type);
            }
        }

        // Register validators
        foreach (var type in types)
        {
            var validatorInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .ToList();

            foreach (var validatorInterface in validatorInterfaces)
            {
                services.AddScoped(validatorInterface, type);
            }
        }
    }
}