using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.Models;

public sealed class ProductVariantModel
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string? Name { get; init; }
    public decimal Price { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public int StockQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public int AvailableQuantity { get; init; }
    public ICollection<ProductVariantSpecificationModel> Specifications { get; init; } = [];
}

public sealed class CreateProductVariantModel
{
    public string Sku { get; init; } = string.Empty;
    public string? Name { get; init; }
    public decimal Price { get; init; }
    public string CurrencyCode { get; init; } = "USD";
    public bool IsDefault { get; init; }
}

public sealed class UpdateProductVariantModel
{
    public string? Sku { get; init; }
    public string? Name { get; init; }
    public decimal? Price { get; init; }
    public string? CurrencyCode { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsDefault { get; init; }
}

public sealed class CreateProductVariantSpecificationModel : SpecificationValueInputModel
{
    public Guid PropertyId { get; init; }
}

public sealed class UpdateProductVariantSpecificationModel : SpecificationValueInputModel
{
}

public sealed class ProductVariantSpecificationModel
{
    public Guid Id { get; init; }
    public Guid ProductVariantId { get; init; }
    public Guid PropertyId { get; init; }
    public string PropertyCode { get; init; } = string.Empty;
    public string PropertyName { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public UnitDefinitionModel? Unit { get; init; }
    public Guid? ValueId { get; init; }
    public SpecificationOptionModel? Option { get; init; }
    public string? TextValue { get; init; }
    public decimal? NumericValue { get; init; }
    public decimal? MinNumericValue { get; init; }
    public decimal? MaxNumericValue { get; init; }
    public bool? BooleanValue { get; init; }
}
