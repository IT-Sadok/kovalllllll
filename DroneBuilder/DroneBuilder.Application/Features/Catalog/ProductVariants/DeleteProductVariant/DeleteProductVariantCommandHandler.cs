using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.DeleteProductVariant;

public sealed class DeleteProductVariantCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<DeleteProductVariantCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        ProductVariant? variant = product.Variants.FirstOrDefault(item => item.Id == command.VariantId);
        if (variant is null)
        {
            return Result.Fail(new NotFoundError(
                $"Variant with id {command.VariantId} not found on product {command.ProductId}."));
        }

        try
        {
            product.DeactivateVariant(variant);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail(new ConflictError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteProductVariantCommand(Guid ProductId, Guid VariantId);
