using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.CreateProductSpecification;

public sealed class CreateProductSpecificationCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<CreateProductSpecificationCommand, ProductSpecificationModel>
{
    public async Task<Result<ProductSpecificationModel>> ExecuteCommandAsync(
        CreateProductSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductSpecificationModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        if (product.ComponentType is null)
        {
            return Result.Fail<ProductSpecificationModel>(new ConflictError(
                "A component type must be assigned before adding typed specifications."));
        }

        Property? property = await repository.GetPropertyAsync(
            command.Model.PropertyId,
            cancellationToken);
        if (property is null)
        {
            return Result.Fail<ProductSpecificationModel>(new NotFoundError(
                $"Property with id {command.Model.PropertyId} not found."));
        }

        ComponentTypeProperty? rule = product.ComponentType.Properties.FirstOrDefault(
            item => item.PropertyId == property.Id);
        if (rule is null)
        {
            return Result.Fail<ProductSpecificationModel>(new ValidationError(
                $"Property '{property.Code}' is not allowed for component type '{product.ComponentType.Code}'."));
        }

        if (rule.IsVariantSpecific)
        {
            return Result.Fail<ProductSpecificationModel>(new ValidationError(
                $"Property '{property.Code}' must be assigned at variant level."));
        }

        if (!property.AllowsMultipleValues &&
            product.ProductPropertyValues.Any(item => item.PropertyId == property.Id))
        {
            return Result.Fail<ProductSpecificationModel>(new ConflictError(
                $"Property '{property.Code}' allows only one value per product."));
        }

        if (ProductSpecificationLogic.HasDuplicate(
                product.ProductPropertyValues,
                property.Id,
                command.Model))
        {
            return Result.Fail<ProductSpecificationModel>(new ConflictError(
                $"The same value is already assigned to property '{property.Code}'."));
        }

        Result<Value?> optionResult = ProductSpecificationLogic.ResolveOption(property, command.Model);
        if (optionResult.IsFailed)
        {
            return optionResult.ToResult<ProductSpecificationModel>();
        }

        var specification = new ProductPropertyValue
        {
            ProductId = product.Id,
            Product = product,
            PropertyId = property.Id,
            Property = property
        };

        try
        {
            specification.SetValue(command.Model, optionResult.Value);
            product.AddSpecification(specification);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<ProductSpecificationModel>(new ValidationError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(specification.ToModel());
    }
}

public sealed record CreateProductSpecificationCommand(
    Guid ProductId,
    CreateProductSpecificationModel Model);
