using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.UpdateProductSpecification;

public sealed class UpdateProductSpecificationCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<UpdateProductSpecificationCommand, ProductSpecificationModel>
{
    public async Task<Result<ProductSpecificationModel>> ExecuteCommandAsync(
        UpdateProductSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductSpecificationModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        ProductPropertyValue? specification = product.ProductPropertyValues.FirstOrDefault(
            item => item.Id == command.SpecificationId);
        if (specification is null)
        {
            return Result.Fail<ProductSpecificationModel>(new NotFoundError(
                $"Specification with id {command.SpecificationId} not found on product {command.ProductId}."));
        }

        Property property = specification.Property
            ?? throw new InvalidOperationException("Specification property metadata is not loaded.");

        if (ProductSpecificationLogic.HasDuplicate(
                product.ProductPropertyValues,
                property.Id,
                command.Model,
                specification.Id))
        {
            return Result.Fail<ProductSpecificationModel>(new ConflictError(
                $"The same value is already assigned to property '{property.Code}'."));
        }

        Result<Value?> optionResult = ProductSpecificationLogic.ResolveOption(property, command.Model);
        if (optionResult.IsFailed)
        {
            return optionResult.ToResult<ProductSpecificationModel>();
        }

        try
        {
            specification.SetValue(command.Model, optionResult.Value);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<ProductSpecificationModel>(new ValidationError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(specification.ToModel());
    }
}

public sealed record UpdateProductSpecificationCommand(
    Guid ProductId,
    Guid SpecificationId,
    UpdateProductSpecificationModel Model);
