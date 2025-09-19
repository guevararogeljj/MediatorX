using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MediatorX.Tests.TestData;
using Xunit;

namespace MediatorX.Tests;

public class MediatorTests
{
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddMediatorX(typeof(MediatorTests).Assembly);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Send_ValidCommand_ExecutesHandler()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var typedHandler = serviceProvider.GetRequiredService<IRequestHandler<TestCommand>>() as TestCommandHandler;
        var command = new TestCommand { Name = "Test", Value = 10 };

        // Act
        await mediator.Send(command);

        // Assert
        Assert.NotNull(typedHandler);
        Assert.True(typedHandler.WasCalled);
    }

    [Fact]
    public async Task Send_ValidQuery_ReturnsResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var query = new TestQuery { Id = 1 };

        // Act
        var result = await mediator.Send<TestQueryResult>(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Data for 1", result.Data);
    }

    [Fact]
    public async Task Send_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand { Name = "", Value = -5 }; // Invalid command

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(command));
        Assert.False(exception.ValidationResult.IsValid);
        Assert.Equal(2, exception.ValidationResult.Errors.Count);
        Assert.Contains(exception.ValidationResult.Errors, e => e.PropertyName == "Name");
        Assert.Contains(exception.ValidationResult.Errors, e => e.PropertyName == "Value");
    }

    [Fact]
    public async Task Send_InvalidQuery_ThrowsValidationException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var query = new TestQuery { Id = 0 }; // Invalid query

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(query));
        Assert.False(exception.ValidationResult.IsValid);
        Assert.Single(exception.ValidationResult.Errors);
        Assert.Equal("Id", exception.ValidationResult.Errors[0].PropertyName);
    }

    [Fact]
    public async Task Validate_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand { Name = "Test", Value = 10 };

        // Act
        var result = await mediator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_InvalidRequest_ReturnsFailure()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand { Name = "", Value = -5 };

        // Act
        var result = await mediator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public async Task Validate_NullRequest_ReturnsFailure()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Validate<TestCommand>(null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Request", result.Errors[0].PropertyName);
        Assert.Equal("Request cannot be null", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Send_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Send((TestCommand)null!));
    }

    [Fact]
    public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Mediator(null!));
    }
}