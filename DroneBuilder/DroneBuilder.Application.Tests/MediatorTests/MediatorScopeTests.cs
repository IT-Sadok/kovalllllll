using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DroneBuilder.Application.Tests.MediatorTests;

public class MediatorScopeTests
{
    private static readonly Guid ValueId = Guid.NewGuid();

    private static ServiceProvider BuildProvider(IValueRepository repository, out Func<int> scopedResolutions)
    {
        var count = 0;

        var services = new ServiceCollection();
        services.AddApplication();

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

        scope.ServiceProvider.GetRequiredService<IValueRepository>();
        Assert.Equal(1, scopedResolutions());

        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        Result result = await mediator.ExecuteCommandAsync(new DeleteValueCommand(ValueId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

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

        // Act
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
