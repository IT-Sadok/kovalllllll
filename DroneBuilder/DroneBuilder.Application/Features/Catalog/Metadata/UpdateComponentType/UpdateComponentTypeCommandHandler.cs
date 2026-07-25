using DroneBuilder.Application.Common;
using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.UpdateComponentType;

public sealed class UpdateComponentTypeCommandHandler(ICatalogMetadataRepository repository)
    : ICommandHandler<UpdateComponentTypeCommand, ComponentTypeModel>
{
    public async Task<Result<ComponentTypeModel>> ExecuteCommandAsync(
        UpdateComponentTypeCommand command,
        CancellationToken cancellationToken)
    {
        ComponentType? componentType = await repository.GetComponentTypeAsync(
            command.ComponentTypeId,
            cancellationToken);
        if (componentType is null)
        {
            return Result.Fail<ComponentTypeModel>(new NotFoundError(
                $"Component type with id {command.ComponentTypeId} not found."));
        }

        string targetCode = command.Model.Code is null
            ? componentType.Code
            : EntityCode.FromName(command.Model.Code);
        if (await repository.IsComponentTypeCodeInUseAsync(targetCode, componentType.Id, cancellationToken))
        {
            return Result.Fail<ComponentTypeModel>(new ConflictError(
                $"Component type code '{targetCode}' already exists."));
        }

        if (command.Model.IsActive == false && componentType.Products.Any(product => product.IsActive))
        {
            return Result.Fail<ComponentTypeModel>(new ConflictError(
                "A component type used by active products cannot be deactivated."));
        }

        componentType.Code = targetCode;
        componentType.Name = command.Model.Name?.Trim() ?? componentType.Name;
        componentType.IsActive = command.Model.IsActive ?? componentType.IsActive;
        componentType.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(componentType.ToModel());
    }
}

public sealed record UpdateComponentTypeCommand(Guid ComponentTypeId, UpdateComponentTypeModel Model);
