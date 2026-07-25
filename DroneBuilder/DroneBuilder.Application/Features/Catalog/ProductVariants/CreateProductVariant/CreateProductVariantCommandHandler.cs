using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.CreateProductVariant;

public sealed class CreateProductVariantCommandHandler(
    ICatalogSpecificationRepository catalogRepository,
    IWarehouseRepository warehouseRepository)
    : ICommandHandler<CreateProductVariantCommand, ProductVariantModel>
{
    public async Task<Result<ProductVariantModel>> ExecuteCommandAsync(
        CreateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await catalogRepository.GetProductAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductVariantModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        string sku = command.Model.Sku.Trim();
        if (await catalogRepository.IsSkuInUseAsync(sku, cancellationToken: cancellationToken))
        {
            return Result.Fail<ProductVariantModel>(new ConflictError($"SKU '{sku}' already exists."));
        }

        Warehouse? warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
        if (warehouse is null)
        {
            return Result.Fail<ProductVariantModel>(new NotFoundError("Warehouse not found."));
        }

        var variant = new ProductVariant
        {
            Sku = sku,
            Name = string.IsNullOrWhiteSpace(command.Model.Name) ? null : command.Model.Name.Trim(),
            Price = command.Model.Price,
            CurrencyCode = command.Model.CurrencyCode.Trim().ToUpperInvariant(),
            IsDefault = false
        };

        product.AddVariant(variant);
        if (command.Model.IsDefault)
        {
            product.SetDefaultVariant(variant);
        }

        var warehouseItem = new WarehouseItem { WarehouseId = warehouse.Id, Warehouse = warehouse };
        warehouseItem.AttachVariant(variant);
        await warehouseRepository.AddWarehouseItemAsync(warehouseItem, cancellationToken);
        await catalogRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(variant.ToModel());
    }
}

public sealed record CreateProductVariantCommand(Guid ProductId, CreateProductVariantModel Model);
