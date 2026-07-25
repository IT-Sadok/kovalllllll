namespace DroneBuilder.Domain.Entities;

public class ComponentTypeProperty : AuditableEntity
{
    public Guid ComponentTypeId { get; set; }
    public ComponentType? ComponentType { get; set; }
    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVariantSpecific { get; set; }
    public int SortOrder { get; set; }
}
