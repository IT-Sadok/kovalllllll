using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.UpsertVariantExternalReference;

public sealed class UpsertVariantExternalReferenceCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<UpsertVariantExternalReferenceCommand, ExternalReferenceModel>
{
    public async Task<Result<ExternalReferenceModel>> ExecuteCommandAsync(
        UpsertVariantExternalReferenceCommand command,
        CancellationToken cancellationToken)
    {
        ImportSource? source = await repository.GetSourceAsync(command.SourceId, cancellationToken);
        if (source is null)
        {
            return Result.Fail<ExternalReferenceModel>(new NotFoundError(
                $"Import source with id {command.SourceId} not found."));
        }

        if (!source.IsActive)
        {
            return Result.Fail<ExternalReferenceModel>(new ConflictError("Import source is inactive."));
        }

        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        ProductVariant? variant = product?.Variants.FirstOrDefault(item => item.Id == command.VariantId);
        if (variant is null)
        {
            return Result.Fail<ExternalReferenceModel>(new NotFoundError(
                $"Variant with id {command.VariantId} not found."));
        }

        ProductVariantExternalReference? reference = await repository.GetVariantReferenceAsync(
            command.SourceId,
            command.Model.ExternalId,
            cancellationToken);
        if (reference is not null && reference.ProductVariantId != variant.Id)
        {
            return Result.Fail<ExternalReferenceModel>(new ConflictError(
                "This source/external ID is already assigned to another variant."));
        }

        if (reference is null)
        {
            reference = new ProductVariantExternalReference
            {
                ImportSourceId = source.Id,
                ImportSource = source,
                ProductVariantId = variant.Id,
                ProductVariant = variant,
                ExternalId = command.Model.ExternalId.Trim()
            };
            await repository.AddVariantReferenceAsync(reference, cancellationToken);
        }

        reference.SourceUrl = string.IsNullOrWhiteSpace(command.Model.SourceUrl)
            ? null
            : command.Model.SourceUrl.Trim();
        reference.ContentHash = string.IsNullOrWhiteSpace(command.Model.ContentHash)
            ? null
            : command.Model.ContentHash.Trim();
        reference.LastSeenAt = DateTime.UtcNow;
        reference.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(reference.ToModel());
    }
}

public sealed record UpsertVariantExternalReferenceCommand(
    Guid ProductId,
    Guid VariantId,
    Guid SourceId,
    UpsertExternalReferenceModel Model);
