using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.DeleteProductSpecification;

public sealed class DeleteProductSpecificationCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<DeleteProductSpecificationCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteProductSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        ProductPropertyValue? specification = product.ProductPropertyValues.FirstOrDefault(
            item => item.Id == command.SpecificationId);
        if (specification is null)
        {
            return Result.Fail(new NotFoundError(
                $"Specification with id {command.SpecificationId} not found on product {command.ProductId}."));
        }

        try
        {
            product.RemoveSpecification(specification);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail(new ConflictError(exception.Message));
        }

        repository.RemoveSpecification(specification);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteProductSpecificationCommand(Guid ProductId, Guid SpecificationId);
