using DroneBuilder.Application.Features.Catalog.ProductSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.CreateVariantSpecification;

public sealed class CreateVariantSpecificationCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<CreateVariantSpecificationCommand, ProductVariantSpecificationModel>
{
    public async Task<Result<ProductVariantSpecificationModel>> ExecuteCommandAsync(
        CreateVariantSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        ProductVariant? variant = product.Variants.FirstOrDefault(
            item => item.Id == command.VariantId && item.IsActive);
        if (variant is null)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new NotFoundError(
                $"Variant with id {command.VariantId} not found."));
        }

        if (product.ComponentType is null)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new ConflictError(
                "A component type must be assigned before adding typed specifications."));
        }

        Property? property = await repository.GetPropertyAsync(command.Model.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new NotFoundError(
                $"Property with id {command.Model.PropertyId} not found."));
        }

        ComponentTypeProperty? rule = product.ComponentType.Properties.FirstOrDefault(
            item => item.PropertyId == property.Id);
        if (rule is null)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new ValidationError(
                $"Property '{property.Code}' is not allowed for component type '{product.ComponentType.Code}'."));
        }

        if (!rule.IsVariantSpecific)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new ValidationError(
                $"Property '{property.Code}' must be assigned at product level."));
        }

        if (!property.AllowsMultipleValues &&
            variant.Specifications.Any(item => item.PropertyId == property.Id))
        {
            return Result.Fail<ProductVariantSpecificationModel>(new ConflictError(
                $"Property '{property.Code}' allows only one value per variant."));
        }

        if (ProductSpecificationLogic.HasDuplicate(variant.Specifications, property.Id, command.Model))
        {
            return Result.Fail<ProductVariantSpecificationModel>(new ConflictError(
                $"The same value is already assigned to property '{property.Code}'."));
        }

        Result<Value?> optionResult = ProductSpecificationLogic.ResolveOption(property, command.Model);
        if (optionResult.IsFailed)
        {
            return optionResult.ToResult<ProductVariantSpecificationModel>();
        }

        var specification = new ProductVariantPropertyValue
        {
            ProductVariantId = variant.Id,
            ProductVariant = variant,
            PropertyId = property.Id,
            Property = property
        };

        try
        {
            specification.SetValue(command.Model, optionResult.Value);
            variant.AddSpecification(specification);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new ValidationError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(specification.ToModel());
    }
}

public sealed record CreateVariantSpecificationCommand(
    Guid ProductId,
    Guid VariantId,
    CreateProductVariantSpecificationModel Model);
