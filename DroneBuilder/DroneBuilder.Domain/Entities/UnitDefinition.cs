namespace DroneBuilder.Domain.Entities;

public class UnitDefinition : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? Dimension { get; set; }
    public decimal ConversionFactorToBase { get; set; } = 1m;
    public ICollection<Property> Properties { get; set; } = [];
    public ICollection<UnitAlias> Aliases { get; set; } = [];

    public decimal ConvertToBase(decimal value) => value * ConversionFactorToBase;
}
