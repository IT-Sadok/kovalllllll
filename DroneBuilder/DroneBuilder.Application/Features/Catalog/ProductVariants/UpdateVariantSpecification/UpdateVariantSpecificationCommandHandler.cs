using DroneBuilder.Application.Features.Catalog.ProductSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.UpdateVariantSpecification;

public sealed class UpdateVariantSpecificationCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<UpdateVariantSpecificationCommand, ProductVariantSpecificationModel>
{
    public async Task<Result<ProductVariantSpecificationModel>> ExecuteCommandAsync(
        UpdateVariantSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        ProductVariant? variant = product.Variants.FirstOrDefault(item => item.Id == command.VariantId);
        ProductVariantPropertyValue? specification = variant?.Specifications.FirstOrDefault(
            item => item.Id == command.SpecificationId);
        if (variant is null || specification is null)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new NotFoundError(
                $"Specification with id {command.SpecificationId} not found on variant {command.VariantId}."));
        }

        Property property = specification.Property
            ?? throw new InvalidOperationException("Variant specification property metadata is not loaded.");
        if (ProductSpecificationLogic.HasDuplicate(
                variant.Specifications,
                property.Id,
                command.Model,
                specification.Id))
        {
            return Result.Fail<ProductVariantSpecificationModel>(new ConflictError(
                $"The same value is already assigned to property '{property.Code}'."));
        }

        Result<Value?> optionResult = ProductSpecificationLogic.ResolveOption(property, command.Model);
        if (optionResult.IsFailed)
        {
            return optionResult.ToResult<ProductVariantSpecificationModel>();
        }

        try
        {
            specification.SetValue(command.Model, optionResult.Value);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<ProductVariantSpecificationModel>(new ValidationError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(specification.ToModel());
    }
}

public sealed record UpdateVariantSpecificationCommand(
    Guid ProductId,
    Guid VariantId,
    Guid SpecificationId,
    UpdateProductVariantSpecificationModel Model);
