using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility;

public record BuildPart(
    Guid ProductId,
    string Name,
    ProductCategory Category,
    ComponentSpec? Spec,
    decimal Price,
    decimal? WeightGrams,
    int Quantity);

public record SpecPart<TSpec>(BuildPart Part, TSpec Spec) where TSpec : ComponentSpec
{
    public Guid Id => Part.ProductId;
    public string Name => Part.Name;
}

public class BuildParts(IReadOnlyList<BuildPart> parts)
{
    public IReadOnlyList<BuildPart> All => parts;

    public IReadOnlyList<BuildPart> InCategory(ProductCategory category)
        => parts.Where(p => p.Category == category).ToList();

    public IReadOnlyList<SpecPart<TSpec>> With<TSpec>() where TSpec : ComponentSpec
        => parts.Where(p => p.Spec is TSpec).Select(p => new SpecPart<TSpec>(p, (TSpec)p.Spec!)).ToList();

    public int MotorCount => InCategory(ProductCategory.Motor).Sum(p => p.Quantity);
}
