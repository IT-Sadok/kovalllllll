namespace DroneBuilder.Domain.Entities;

public class PropertyAlias : AuditableEntity
{
    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }
    public Guid? ImportSourceId { get; set; }
    public ImportSource? ImportSource { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string NormalizedAlias { get; set; } = string.Empty;

    public void Normalize() => NormalizedAlias = SpecificationAliasNormalizer.Normalize(Alias);
}
