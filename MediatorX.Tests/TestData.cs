using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediatorX.Tests.TestData;

// Test requests
public class TestCommand : IRequest
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class TestQuery : IRequest<TestQueryResult>
{
    public int Id { get; set; }
}

public class TestQueryResult
{
    public int Id { get; set; }
    public string Data { get; set; } = string.Empty;
}

// Test handlers
public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public bool WasCalled { get; private set; }

    public Task Handle(TestCommand request, CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        return Task.CompletedTask;
    }
}

public class TestQueryHandler : IRequestHandler<TestQuery, TestQueryResult>
{
    public Task<TestQueryResult> Handle(TestQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TestQueryResult 
        { 
            Id = request.Id, 
            Data = $"Data for {request.Id}" 
        });
    }
}

// Test validators
public class TestCommandValidator : IValidator<TestCommand>
{
    public Task<ValidationResult> ValidateAsync(TestCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "Name is required"));

        if (request.Value < 0)
            errors.Add(new ValidationError(nameof(request.Value), "Value must be non-negative"));

        return Task.FromResult(errors.Any() ? new ValidationResult(errors) : ValidationResult.Success());
    }
}

public class TestQueryValidator : IValidator<TestQuery>
{
    public Task<ValidationResult> ValidateAsync(TestQuery request, CancellationToken cancellationToken = default)
    {
        if (request.Id <= 0)
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(nameof(request.Id), "Id must be greater than 0")));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}