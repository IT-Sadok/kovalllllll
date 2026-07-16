using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.ResultErrors;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Application.Mediator;

public class Mediator(IServiceScopeFactory scopeFactory) : IMediator
{
    public async Task<Result> ExecuteCommandAsync<T>(T command, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        Result validationResult = await ValidateAsync(scope.ServiceProvider, command, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        ICommandHandler<T> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>();

        return await handler.ExecuteCommandAsync(command, cancellationToken);
    }

    public async Task<Result> ExecuteQueryAsync<T>(T query, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        Result validationResult = await ValidateAsync(scope.ServiceProvider, query, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        IQueryHandler<T> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<T>>();

        return await handler.ExecuteAsync(query, cancellationToken);
    }

    public async Task<Result<TResult>> ExecuteCommandAsync<T, TResult>(T command, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        Result validationResult = await ValidateAsync(scope.ServiceProvider, command, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        ICommandHandler<T, TResult> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T, TResult>>();

        return await handler.ExecuteCommandAsync(command, cancellationToken);
    }

    public async Task<Result<TResult>> ExecuteQueryAsync<T, TResult>(T query, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        Result validationResult = await ValidateAsync(scope.ServiceProvider, query, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        IQueryHandler<T, TResult> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<T, TResult>>();

        return await handler.ExecuteAsync(query, cancellationToken);
    }

    private static async Task<Result> ValidateAsync<T>(IServiceProvider serviceProvider, T instance,
        CancellationToken cancellationToken)
    {
        IValidator<T>? validator = serviceProvider.GetService<IValidator<T>>();
        if (validator is null)
        {
            return Result.Ok();
        }

        ValidationResult validationResult = await validator.ValidateAsync(instance, cancellationToken);
        if (validationResult.IsValid)
        {
            return Result.Ok();
        }

        IDictionary<string, string[]> errorDetails = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return Result.Fail(new ValidationError("Validation failed.", errorDetails));
    }
}

