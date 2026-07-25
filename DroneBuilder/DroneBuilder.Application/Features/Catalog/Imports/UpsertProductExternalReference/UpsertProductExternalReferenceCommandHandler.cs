using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.UpsertProductExternalReference;

public sealed class UpsertProductExternalReferenceCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<UpsertProductExternalReferenceCommand, ExternalReferenceModel>
{
    public async Task<Result<ExternalReferenceModel>> ExecuteCommandAsync(
        UpsertProductExternalReferenceCommand command,
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
        if (product is null)
        {
            return Result.Fail<ExternalReferenceModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        ProductExternalReference? reference = await repository.GetProductReferenceAsync(
            command.SourceId,
            command.Model.ExternalId,
            cancellationToken);
        if (reference is not null && reference.ProductId != product.Id)
        {
            return Result.Fail<ExternalReferenceModel>(new ConflictError(
                "This source/external ID is already assigned to another product."));
        }

        if (reference is null)
        {
            reference = new ProductExternalReference
            {
                ImportSourceId = source.Id,
                ImportSource = source,
                ProductId = product.Id,
                Product = product,
                ExternalId = command.Model.ExternalId.Trim()
            };
            await repository.AddProductReferenceAsync(reference, cancellationToken);
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

public sealed record UpsertProductExternalReferenceCommand(
    Guid ProductId,
    Guid SourceId,
    UpsertExternalReferenceModel Model);
