using DroneBuilder.Application.Features.Catalog.Metadata.Models;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;

public class SpecificationValueInputModel
{
    public Guid? ValueId { get; init; }
    public string? TextValue { get; init; }
    public decimal? NumericValue { get; init; }
    public decimal? MinNumericValue { get; init; }
    public decimal? MaxNumericValue { get; init; }
    public bool? BooleanValue { get; init; }
}

public sealed class CreateProductSpecificationModel : SpecificationValueInputModel
{
    public Guid PropertyId { get; init; }
}

public sealed class UpdateProductSpecificationModel : SpecificationValueInputModel
{
}

public sealed class AssignProductComponentTypeModel
{
    public Guid ComponentTypeId { get; init; }
}

public sealed class ProductComponentTypeModel
{
    public Guid ProductId { get; init; }
    public string ProductKind { get; init; } = string.Empty;
    public ComponentTypeModel ComponentType { get; init; } = new();
}

public sealed class ProductSpecificationModel
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
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
