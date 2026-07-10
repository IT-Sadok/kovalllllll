using DroneBuilder.Application.Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Application.Mediator;

public class Mediator(IServiceScopeFactory scopeFactory) : IMediator
{
    public async Task ExecuteCommandAsync<T>(T command, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ICommandHandler<T> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>();

        await handler.ExecuteCommandAsync(command, cancellationToken);
    }

    public async Task ExecuteQueryAsync<T>(T query, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IQueryHandler<T> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<T>>();

        await handler.ExecuteAsync(query, cancellationToken);
    }

    public async Task<TResult> ExecuteCommandAsync<T, TResult>(T command, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ICommandHandler<T, TResult> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T, TResult>>();

        return await handler.ExecuteCommandAsync(command, cancellationToken);
    }

    public async Task<TResult> ExecuteQueryAsync<T, TResult>(T query, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IQueryHandler<T, TResult> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<T, TResult>>();

        return await handler.ExecuteAsync(query, cancellationToken);
    }
}
