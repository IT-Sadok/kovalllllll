using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Products.PublishProduct;

public sealed class PublishProductCommandHandler(IProductRepository repository)
    : ICommandHandler<PublishProductCommand, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteCommandAsync(
        PublishProductCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductModel>(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        try
        {
            product.Publish();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<ProductModel>(new ConflictError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(product.ToModel());
    }
}

public sealed record PublishProductCommand(Guid ProductId);
