using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.Models;

public static class ProductVariantMappings
{
    public static ProductVariantModel ToModel(this ProductVariant variant)
    {
        return new ProductVariantModel
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            Sku = variant.Sku,
            Name = variant.Name,
            Price = variant.Price,
            CurrencyCode = variant.CurrencyCode,
            IsDefault = variant.IsDefault,
            IsActive = variant.IsActive,
            StockQuantity = variant.WarehouseItems.Sum(item => item.Quantity),
            ReservedQuantity = variant.WarehouseItems.Sum(item => item.ReservedQuantity),
            AvailableQuantity = variant.WarehouseItems.Sum(item => item.AvailableQuantity),
            Specifications = variant.Specifications
                .OrderBy(specification => specification.Property!.Name)
                .Select(specification => specification.ToModel())
                .ToList()
        };
    }

    public static ProductVariantSpecificationModel ToModel(
        this ProductVariantPropertyValue specification)
    {
        Property property = specification.Property
            ?? throw new InvalidOperationException("Variant specification property metadata is not loaded.");

        return new ProductVariantSpecificationModel
        {
            Id = specification.Id,
            ProductVariantId = specification.ProductVariantId,
            PropertyId = property.Id,
            PropertyCode = property.Code,
            PropertyName = property.Name,
            DataType = property.DataType.ToString(),
            Unit = property.UnitDefinition?.ToModel(),
            ValueId = specification.ValueId,
            Option = specification.Value?.ToModel(),
            TextValue = specification.TextValue,
            NumericValue = specification.NumericValue,
            MinNumericValue = specification.MinNumericValue,
            MaxNumericValue = specification.MaxNumericValue,
            BooleanValue = specification.BooleanValue
        };
    }

    public static void SetValue(
        this ProductVariantPropertyValue specification,
        SpecificationValueInputModel model,
        Value? option)
    {
        specification.SetValue(
            model.ValueId,
            option,
            model.TextValue,
            model.NumericValue,
            model.MinNumericValue,
            model.MaxNumericValue,
            model.BooleanValue);
    }
}
