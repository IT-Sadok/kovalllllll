namespace DroneBuilder.Domain.Entities;

public class UnitAlias : AuditableEntity
{
    public Guid UnitDefinitionId { get; set; }
    public UnitDefinition? UnitDefinition { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string NormalizedAlias { get; set; } = string.Empty;

    public void Normalize() => NormalizedAlias = SpecificationAliasNormalizer.Normalize(Alias);
}
