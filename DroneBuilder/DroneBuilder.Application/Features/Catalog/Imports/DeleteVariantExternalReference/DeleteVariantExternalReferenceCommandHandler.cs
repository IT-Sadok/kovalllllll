using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.DeleteVariantExternalReference;

public sealed class DeleteVariantExternalReferenceCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<DeleteVariantExternalReferenceCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteVariantExternalReferenceCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        ProductVariant? variant = product?.Variants.FirstOrDefault(item => item.Id == command.VariantId);
        ProductVariantExternalReference? reference = variant?.ExternalReferences.FirstOrDefault(
            item => item.Id == command.ReferenceId);
        if (variant is null || reference is null)
        {
            return Result.Fail(new NotFoundError($"External reference with id {command.ReferenceId} not found."));
        }

        repository.RemoveVariantReference(reference);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteVariantExternalReferenceCommand(
    Guid ProductId,
    Guid VariantId,
    Guid ReferenceId);
