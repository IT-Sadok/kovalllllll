using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.DeleteProductExternalReference;

public sealed class DeleteProductExternalReferenceCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<DeleteProductExternalReferenceCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteProductExternalReferenceCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        ProductExternalReference? reference = product?.ExternalReferences.FirstOrDefault(
            item => item.Id == command.ReferenceId);
        if (product is null || reference is null)
        {
            return Result.Fail(new NotFoundError($"External reference with id {command.ReferenceId} not found."));
        }

        repository.RemoveProductReference(reference);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteProductExternalReferenceCommand(Guid ProductId, Guid ReferenceId);
