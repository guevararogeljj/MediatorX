using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorX.Examples;

// Example domain objects
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Example requests
public class CreateUserCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}

public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}

// Example handlers
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private static readonly List<User> _users = new();
    private static int _nextId = 1;

    public Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Id = _nextId++,
            Name = request.Name,
            Email = request.Email
        };

        _users.Add(user);
        return Task.FromResult(user.Id);
    }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    private static readonly List<User> _users = new();

    public Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == request.UserId);
        return Task.FromResult(user ?? new User());
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private static readonly List<User> _users = new();

    public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == request.UserId);
        if (user != null)
        {
            _users.Remove(user);
        }
        return Task.CompletedTask;
    }
}

// Example validators
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "Name is required"));

        if (request.Name?.Length > 100)
            errors.Add(new ValidationError(nameof(request.Name), "Name must be 100 characters or less"));

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add(new ValidationError(nameof(request.Email), "Email is required"));

        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains("@"))
            errors.Add(new ValidationError(nameof(request.Email), "Email must be valid"));

        return Task.FromResult(errors.Any() ? new ValidationResult(errors) : ValidationResult.Success());
    }
}

public class GetUserQueryValidator : IValidator<GetUserQuery>
{
    public Task<ValidationResult> ValidateAsync(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0)
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(nameof(request.UserId), "UserId must be greater than 0")));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

public class DeleteUserCommandValidator : IValidator<DeleteUserCommand>
{
    public Task<ValidationResult> ValidateAsync(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0)
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(nameof(request.UserId), "UserId must be greater than 0")));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

// Example usage
public class ExampleUsage
{
    public static async Task RunExample()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddMediatorX(typeof(ExampleUsage).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        try
        {
            // Example 1: Create a user (valid request)
            var createCommand = new CreateUserCommand 
            { 
                Name = "John Doe", 
                Email = "john.doe@example.com" 
            };

            var userId = await mediator.Send<int>(createCommand);
            Console.WriteLine($"Created user with ID: {userId}");

            // Example 2: Get a user
            var getQuery = new GetUserQuery { UserId = userId };
            var user = await mediator.Send<User>(getQuery);
            Console.WriteLine($"Retrieved user: {user.Name} ({user.Email})");

            // Example 3: Try to create an invalid user (validation will fail)
            var invalidCommand = new CreateUserCommand 
            { 
                Name = "", // Invalid: empty name
                Email = "invalid-email" // Invalid: no @ symbol
            };

            try
            {
                await mediator.Send<int>(invalidCommand);
            }
            catch (ValidationException ex)
            {
                Console.WriteLine("Validation failed:");
                foreach (var error in ex.ValidationResult.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            // Example 4: Validate without sending
            var validationResult = await mediator.Validate(invalidCommand);
            Console.WriteLine($"Manual validation result: {(validationResult.IsValid ? "Valid" : "Invalid")}");

            // Example 5: Delete the user
            var deleteCommand = new DeleteUserCommand { UserId = userId };
            await mediator.Send(deleteCommand);
            Console.WriteLine("User deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}