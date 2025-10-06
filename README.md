# MediatorX

A lightweight .NET mediator pattern implementation with built-in validation support.

## Features

- **Built-in Validation**: Every request is automatically validated before execution
- **Type-safe Request/Response**: Strongly typed request and response patterns
- **Dependency Injection**: Full integration with Microsoft.Extensions.DependencyInjection
- **Async Support**: All operations are async by default
- **Minimal Dependencies**: Only depends on Microsoft.Extensions.DependencyInjection.Abstractions

## Installation

```bash
dotnet add package MediatorX
```

## Quick Start

### 1. Define your requests and responses

```csharp
// Request without response
public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}

// Request with response
public class CreateUserCommand : IRequest<int>
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}
```

### 2. Create request handlers

```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Your business logic here
        var user = new User { Name = request.Name, Email = request.Email };
        // Save user to database
        return user.Id;
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Your business logic here
        // Delete user from database
    }
}
```

### 3. Create validators (optional but recommended)

```csharp
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public async Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "Name is required"));

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add(new ValidationError(nameof(request.Email), "Email is required"));

        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains("@"))
            errors.Add(new ValidationError(nameof(request.Email), "Email must be valid"));

        return errors.Any() ? new ValidationResult(errors) : ValidationResult.Success();
    }
}
```

### 4. Register services

```csharp
// In your Program.cs or Startup.cs
services.AddMediatorX(typeof(Program).Assembly);
```

### 5. Use the mediator

```csharp
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send<int>(command);
            return Ok(new { UserId = userId });
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.ValidationResult.Errors);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _mediator.Send(new DeleteUserCommand { UserId = id });
        return NoContent();
    }
}
```

## Validation

MediatorX includes a powerful validation system that automatically validates requests before they are handled.

### Validation Results

```csharp
// Validate without sending
var validationResult = await mediator.Validate(request);

if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

### Automatic Validation

When you call `Send()`, validation happens automatically:

```csharp
try
{
    var result = await mediator.Send<int>(command);
}
catch (ValidationException ex)
{
    // Handle validation errors
    var errors = ex.ValidationResult.Errors;
}
```

### Multiple Validators

You can have multiple validators for the same request type. All validators will be executed, and all errors will be collected:

```csharp
public class CreateUserBusinessRulesValidator : IValidator<CreateUserCommand>
{
    public async Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Additional business rule validation
        // Check if email already exists, etc.
    }
}
```

## Error Handling

### ValidationException

Thrown when validation fails during `Send()` operations:

```csharp
try
{
    await mediator.Send(command);
}
catch (ValidationException ex)
{
    // ex.ValidationResult contains all validation errors
    foreach (var error in ex.ValidationResult.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

### Other Exceptions

- `ArgumentNullException`: Thrown when request is null
- `InvalidOperationException`: Thrown when no handler is found for a request type

## Best Practices

1. **Keep requests simple**: Requests should be simple DTOs containing only the data needed for the operation
2. **Validate early**: Use validators to catch invalid data before it reaches your business logic
3. **Separate concerns**: Keep validation logic separate from business logic
4. **Use specific error messages**: Provide clear, actionable error messages in validators
5. **Handle exceptions appropriately**: Always handle `ValidationException` when calling `Send()`

## Examples

See the `MediatorX.Tests/ExampleUsage.cs` file for comprehensive examples of usage patterns.

## License

MIT License - see LICENSE file for details.
