using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.UpdateProductVariant;

public sealed class UpdateProductVariantCommandHandler(ICatalogSpecificationRepository repository)
    : ICommandHandler<UpdateProductVariantCommand, ProductVariantModel>
{
    public async Task<Result<ProductVariantModel>> ExecuteCommandAsync(
        UpdateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductVariantModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        ProductVariant? variant = product.Variants.FirstOrDefault(item => item.Id == command.VariantId);
        if (variant is null)
        {
            return Result.Fail<ProductVariantModel>(new NotFoundError(
                $"Variant with id {command.VariantId} not found on product {command.ProductId}."));
        }

        string targetSku = command.Model.Sku?.Trim() ?? variant.Sku;
        if (await repository.IsSkuInUseAsync(targetSku, variant.Id, cancellationToken))
        {
            return Result.Fail<ProductVariantModel>(new ConflictError($"SKU '{targetSku}' already exists."));
        }

        try
        {
            if (command.Model.Sku is not null || command.Model.Name is not null ||
                command.Model.Price.HasValue || command.Model.CurrencyCode is not null)
            {
                variant.UpdateDetails(
                    command.Model.Sku,
                    command.Model.Name,
                    command.Model.Price,
                    command.Model.CurrencyCode);
            }

            if (command.Model.IsActive == true && !variant.IsActive)
            {
                product.ActivateVariant(variant);
            }
            else if (command.Model.IsActive == false && variant.IsActive)
            {
                product.DeactivateVariant(variant);
            }

            if (command.Model.IsDefault == true && !variant.IsDefault)
            {
                product.SetDefaultVariant(variant);
            }
            else if (command.Model.IsDefault == false && variant.IsDefault)
            {
                return Result.Fail<ProductVariantModel>(new ConflictError(
                    "Select another default variant instead of unsetting the current default."));
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
        {
            return Result.Fail<ProductVariantModel>(new ConflictError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(variant.ToModel());
    }
}

public sealed record UpdateProductVariantCommand(
    Guid ProductId,
    Guid VariantId,
    UpdateProductVariantModel Model);
