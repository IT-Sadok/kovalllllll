using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Products.RemoveProductSpec;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
namespace DroneBuilder.Application.Tests.Common.Mediator;

public class MediatorScopeTests
{
    private static readonly Guid ProductId = Guid.NewGuid();

    private static ServiceProvider BuildProvider(IProductRepository repository, out Func<int> scopedResolutions)
    {
        int count = 0;

        var services = new ServiceCollection();
        services.AddApplication();

        services.AddScoped<IProductRepository>(_ =>
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
        var product = new Product { Id = ProductId, Spec = new AntennaSpec { ProductId = ProductId } };
        IProductRepository repository = Substitute.For<IProductRepository>();
        repository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        await using ServiceProvider provider = BuildProvider(repository, out Func<int> scopedResolutions);

        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IProductRepository>();
        Assert.Equal(1, scopedResolutions());

        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        Result result = await mediator.ExecuteCommandAsync(new RemoveProductSpecCommand(ProductId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(1, scopedResolutions());

        Assert.Null(product.Spec);
        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_ShouldStillRunTheValidatorFromThatScope()
    {
        // Arrange
        IProductRepository repository = Substitute.For<IProductRepository>();

        await using ServiceProvider provider = BuildProvider(repository, out _);

        using IServiceScope scope = provider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        Result result = await mediator.ExecuteCommandAsync(new RemoveProductSpecCommand(Guid.Empty), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ValidationError>());

        await repository.DidNotReceive().GetProductByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenHandlerFails_ShouldSurfaceTheHandlerError()
    {
        // Arrange
        IProductRepository repository = Substitute.For<IProductRepository>();
        repository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        await using ServiceProvider provider = BuildProvider(repository, out _);

        using IServiceScope scope = provider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        Result result = await mediator.ExecuteCommandAsync(new RemoveProductSpecCommand(ProductId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
    }
}
