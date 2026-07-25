namespace DroneBuilder.Domain.Entities;

public class ValueAlias : AuditableEntity
{
    public Guid ValueId { get; set; }
    public Value? Value { get; set; }
    public Guid? ImportSourceId { get; set; }
    public ImportSource? ImportSource { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string NormalizedAlias { get; set; } = string.Empty;

    public void Normalize() => NormalizedAlias = SpecificationAliasNormalizer.Normalize(Alias);
}
