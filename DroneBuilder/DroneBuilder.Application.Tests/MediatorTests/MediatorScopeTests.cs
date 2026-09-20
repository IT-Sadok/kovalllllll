using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DroneBuilder.Application.Tests.MediatorTests;

/// <summary>
/// The mediator has to resolve handlers from the scope it was resolved from. Creating a child scope
/// per call hands every handler its own DbContext, which silently rules out sharing a transaction
/// between two calls in the same request.
/// </summary>
public class MediatorScopeTests
{
    private static readonly Guid ValueId = Guid.NewGuid();

    private static ServiceProvider BuildProvider(IValueRepository repository, out Func<int> scopedResolutions)
    {
        var count = 0;

        var services = new ServiceCollection();
        services.AddApplication();

        // Scoped, so the factory runs once per scope: a second run means a second scope was created.
        services.AddScoped<IValueRepository>(_ =>
        {
            count++;
            return repository;
        });

        scopedResolutions = () => count;

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteCommandAsync_ShouldResolveTheHandlerFromTheCallersScope()
    {
        // Arrange
        IValueRepository repository = Substitute.For<IValueRepository>();
        repository.GetValueByIdAsync(ValueId, Arg.Any<CancellationToken>())
            .Returns(new Value { Id = ValueId, Text = "Carbon" });

        await using ServiceProvider provider = BuildProvider(repository, out Func<int> scopedResolutions);

        using IServiceScope scope = provider.CreateScope();

        // Touching the repository here creates the scope's single instance.
        scope.ServiceProvider.GetRequiredService<IValueRepository>();
        Assert.Equal(1, scopedResolutions());

        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        Result result = await mediator.ExecuteCommandAsync(new DeleteValueCommand(ValueId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Still one: the handler shared the caller's scope instead of getting a fresh one.
        Assert.Equal(1, scopedResolutions());

        repository.Received(1).RemoveValue(Arg.Is<Value>(v => v.Id == ValueId));
    }

    [Fact]
    public async Task ExecuteCommandAsync_ShouldStillRunTheValidatorFromThatScope()
    {
        // Arrange
        IValueRepository repository = Substitute.For<IValueRepository>();

        await using ServiceProvider provider = BuildProvider(repository, out _);

        using IServiceScope scope = provider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act -- an empty id is rejected by DeleteValueCommandValidator.
        Result result = await mediator.ExecuteCommandAsync(new DeleteValueCommand(Guid.Empty), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ValidationError>());

        await repository.DidNotReceive().GetValueByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenHandlerFails_ShouldSurfaceTheHandlerError()
    {
        // Arrange
        IValueRepository repository = Substitute.For<IValueRepository>();
        repository.GetValueByIdAsync(ValueId, Arg.Any<CancellationToken>()).Returns((Value)null!);

        await using ServiceProvider provider = BuildProvider(repository, out _);

        using IServiceScope scope = provider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        Result result = await mediator.ExecuteCommandAsync(new DeleteValueCommand(ValueId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
    }
}
