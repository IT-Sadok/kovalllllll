namespace DroneBuilder.Domain.Entities;

public class ComponentType : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<ComponentTypeProperty> Properties { get; set; } = [];

    public ComponentTypeProperty RequirePropertyRule(
        Guid propertyId,
        bool variantSpecific)
    {
        ComponentTypeProperty? rule = Properties.FirstOrDefault(
            item => item.PropertyId == propertyId);

        if (rule is null)
        {
            throw new InvalidOperationException(
                $"Property '{propertyId}' is not allowed for component type '{Code}'.");
        }

        if (rule.IsVariantSpecific != variantSpecific)
        {
            string expectedLevel = rule.IsVariantSpecific ? "variant" : "product";
            throw new InvalidOperationException(
                $"Property '{propertyId}' must be assigned at {expectedLevel} level for component type '{Code}'.");
        }

        return rule;
    }
}
