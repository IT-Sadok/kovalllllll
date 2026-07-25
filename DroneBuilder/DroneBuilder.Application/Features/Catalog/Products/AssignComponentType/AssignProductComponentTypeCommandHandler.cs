using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Products.AssignComponentType;

public sealed class AssignProductComponentTypeCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<AssignProductComponentTypeCommand, ProductComponentTypeModel>
{
    public async Task<Result<ProductComponentTypeModel>> ExecuteCommandAsync(
        AssignProductComponentTypeCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductComponentTypeModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        ComponentType? componentType = await repository.GetComponentTypeAsync(
            command.Model.ComponentTypeId,
            cancellationToken);
        if (componentType is null)
        {
            return Result.Fail<ProductComponentTypeModel>(new NotFoundError(
                $"Component type with id {command.Model.ComponentTypeId} not found."));
        }

        try
        {
            product.AssignComponentType(componentType);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<ProductComponentTypeModel>(new ConflictError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(new ProductComponentTypeModel
        {
            ProductId = product.Id,
            ProductKind = product.Kind.ToString(),
            ComponentType = componentType.ToModel()
        });
    }
}

public sealed record AssignProductComponentTypeCommand(
    Guid ProductId,
    AssignProductComponentTypeModel Model);
