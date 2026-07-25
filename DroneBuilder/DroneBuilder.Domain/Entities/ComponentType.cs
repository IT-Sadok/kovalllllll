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
    public ComponentTypeProperty AddPropertyRule(
        Property property,
        bool isRequired,
        bool isVariantSpecific,
        int sortOrder)
    {
        ArgumentNullException.ThrowIfNull(property);
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder));
        }

        if (Properties.Any(rule => rule.PropertyId == property.Id))
        {
            throw new InvalidOperationException(
                $"Property '{property.Code}' is already assigned to component type '{Code}'.");
        }

        var rule = new ComponentTypeProperty
        {
            ComponentTypeId = Id,
            ComponentType = this,
            PropertyId = property.Id,
            Property = property,
            IsRequired = isRequired,
            IsVariantSpecific = isVariantSpecific,
            SortOrder = sortOrder
        };
        Properties.Add(rule);
        UpdatedAt = DateTime.UtcNow;
        return rule;
    }

    public void RemovePropertyRule(ComponentTypeProperty rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        if (rule.ComponentTypeId != Id || !Properties.Contains(rule))
        {
            throw new InvalidOperationException("Property rule does not belong to this component type.");
        }

        bool hasProductValues = Products.Any(product =>
            product.ProductPropertyValues.Any(value => value.PropertyId == rule.PropertyId) ||
            product.Variants.Any(variant =>
                variant.Specifications.Any(value => value.PropertyId == rule.PropertyId)));
        if (hasProductValues)
        {
            throw new InvalidOperationException(
                $"Property '{rule.PropertyId}' is used by products of component type '{Code}'.");
        }

        Properties.Remove(rule);
        UpdatedAt = DateTime.UtcNow;
    }
}
