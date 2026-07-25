using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.DeleteVariantSpecification;

public sealed class DeleteVariantSpecificationCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<DeleteVariantSpecificationCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteVariantSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        ProductVariant? variant = product.Variants.FirstOrDefault(item => item.Id == command.VariantId);
        ProductVariantPropertyValue? specification = variant?.Specifications.FirstOrDefault(
            item => item.Id == command.SpecificationId);
        if (variant is null || specification is null)
        {
            return Result.Fail(new NotFoundError(
                $"Specification with id {command.SpecificationId} not found on variant {command.VariantId}."));
        }

        try
        {
            variant.RemoveSpecification(specification);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail(new ConflictError(exception.Message));
        }

        repository.RemoveVariantSpecification(specification);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteVariantSpecificationCommand(
    Guid ProductId,
    Guid VariantId,
    Guid SpecificationId);
