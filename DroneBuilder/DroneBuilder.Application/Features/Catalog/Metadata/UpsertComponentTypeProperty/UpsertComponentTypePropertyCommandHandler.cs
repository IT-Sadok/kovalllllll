using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.UpsertComponentTypeProperty;

public sealed class UpsertComponentTypePropertyCommandHandler(ICatalogMetadataRepository repository)
    : ICommandHandler<UpsertComponentTypePropertyCommand, ComponentTypePropertyModel>
{
    public async Task<Result<ComponentTypePropertyModel>> ExecuteCommandAsync(
        UpsertComponentTypePropertyCommand command,
        CancellationToken cancellationToken)
    {
        ComponentType? componentType = await repository.GetComponentTypeAsync(
            command.ComponentTypeId,
            cancellationToken);
        if (componentType is null)
        {
            return Result.Fail<ComponentTypePropertyModel>(new NotFoundError(
                $"Component type with id {command.ComponentTypeId} not found."));
        }

        Property? property = await repository.GetPropertyAsync(command.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail<ComponentTypePropertyModel>(new NotFoundError(
                $"Property with id {command.PropertyId} not found."));
        }

        ComponentTypeProperty? rule = componentType.Properties.FirstOrDefault(
            item => item.PropertyId == property.Id);
        bool created = rule is null;
        bool oldRequired = rule?.IsRequired ?? false;
        bool oldVariantSpecific = rule?.IsVariantSpecific ?? false;
        int oldSortOrder = rule?.SortOrder ?? 0;

        try
        {
            if (rule is null)
            {
                rule = componentType.AddPropertyRule(
                    property,
                    command.Model.IsRequired,
                    command.Model.IsVariantSpecific,
                    command.Model.SortOrder);
            }
            else
            {
                bool hasExistingValues = componentType.Products.Any(product =>
                    product.ProductPropertyValues.Any(value => value.PropertyId == property.Id) ||
                    product.Variants.Any(variant =>
                        variant.Specifications.Any(value => value.PropertyId == property.Id)));
                if (hasExistingValues && rule.IsVariantSpecific != command.Model.IsVariantSpecific)
                {
                    return Result.Fail<ComponentTypePropertyModel>(new ConflictError(
                        "A property rule with existing specification values cannot change its assignment level."));
                }

                rule.IsRequired = command.Model.IsRequired;
                rule.IsVariantSpecific = command.Model.IsVariantSpecific;
                rule.SortOrder = command.Model.SortOrder;
                rule.UpdatedAt = DateTime.UtcNow;
            }

            foreach (Product product in componentType.Products.Where(product => product.IsActive))
            {
                product.ValidateForPublication();
            }
        }
        catch (InvalidOperationException exception)
        {
            if (!created && rule is not null)
            {
                rule.IsRequired = oldRequired;
                rule.IsVariantSpecific = oldVariantSpecific;
                rule.SortOrder = oldSortOrder;
            }

            return Result.Fail<ComponentTypePropertyModel>(new ConflictError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(rule!.ToModel());
    }
}

public sealed record UpsertComponentTypePropertyCommand(
    Guid ComponentTypeId,
    Guid PropertyId,
    UpsertComponentTypePropertyModel Model);
