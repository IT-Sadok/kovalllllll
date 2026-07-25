namespace DroneBuilder.Application.Features.Catalog.Metadata.Models;

public class ComponentTypeModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed class ComponentTypeDetailsModel : ComponentTypeModel
{
    public ICollection<ComponentTypePropertyModel> Properties { get; init; } = [];
}

public sealed class CreateComponentTypeModel
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public sealed class UpdateComponentTypeModel
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class UpsertComponentTypePropertyModel
{
    public Guid PropertyId { get; init; }
    public bool IsRequired { get; init; }
    public bool IsVariantSpecific { get; init; }
    public int SortOrder { get; init; }
}

public sealed class ComponentTypePropertyModel
{
    public Guid RuleId { get; init; }
    public Guid PropertyId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public bool IsVariantSpecific { get; init; }
    public bool AllowsMultipleValues { get; init; }
    public bool IsFilterable { get; init; }
    public bool IsCompatibilityRelevant { get; init; }
    public int SortOrder { get; init; }
    public UnitDefinitionModel? Unit { get; init; }
    public ICollection<SpecificationOptionModel> Options { get; init; } = [];
}

public sealed class UnitDefinitionModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public string? Dimension { get; init; }
    public decimal ConversionFactorToBase { get; init; }
    public ICollection<string> Aliases { get; init; } = [];
}

public sealed class CreateUnitDefinitionModel
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public string? Dimension { get; init; }
    public decimal ConversionFactorToBase { get; init; } = 1m;
    public ICollection<string> Aliases { get; init; } = [];
}

public sealed class UpdateUnitDefinitionModel
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Symbol { get; init; }
    public string? Dimension { get; init; }
    public bool ClearDimension { get; init; }
    public decimal? ConversionFactorToBase { get; init; }
    public ICollection<string>? Aliases { get; init; }
}

public sealed class SpecificationOptionModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public decimal? NumericValue { get; init; }
    public bool? BooleanValue { get; init; }
}
