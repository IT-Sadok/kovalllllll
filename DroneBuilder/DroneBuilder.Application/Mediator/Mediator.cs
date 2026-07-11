using DroneBuilder.Application.Mediator.Interfaces;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Application.Mediator;

public class Mediator(IServiceScopeFactory scopeFactory) : IMediator
{
    public async Task<Result> ExecuteCommandAsync<T>(T command, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ICommandHandler<T> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>();

        return await handler.ExecuteCommandAsync(command, cancellationToken);
    }

    public async Task<Result> ExecuteQueryAsync<T>(T query, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IQueryHandler<T> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<T>>();

        return await handler.ExecuteAsync(query, cancellationToken);
    }

    public async Task<Result<TResult>> ExecuteCommandAsync<T, TResult>(T command, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ICommandHandler<T, TResult> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T, TResult>>();

        return await handler.ExecuteCommandAsync(command, cancellationToken);
    }

    public async Task<Result<TResult>> ExecuteQueryAsync<T, TResult>(T query, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IQueryHandler<T, TResult> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<T, TResult>>();

        return await handler.ExecuteAsync(query, cancellationToken);
    }
}
