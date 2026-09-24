using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Domain.Entities;

public static class ProductCategoryExtensions
{
    public static ComponentType? ToComponentType(this ProductCategory category)
        => Enum.IsDefined((ComponentType)(int)category) ? (ComponentType)(int)category : null;
}
