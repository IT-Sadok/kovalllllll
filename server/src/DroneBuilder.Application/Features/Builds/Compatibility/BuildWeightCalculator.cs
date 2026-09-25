using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility;

public record BuildWeight(
    decimal DryGrams,
    decimal? AllUpGrams,
    decimal? ThrustToWeight,
    bool IsComplete,
    IReadOnlyList<Guid> MissingWeightProductIds);

public static class BuildWeightCalculator
{
    private const int DefaultMotorCount = 4;

    public static BuildWeight Calculate(BuildParts build)
    {
        List<BuildPart> installed = build.All
            .Where(p => p.Category.ToComponentType() is not null && p.Category != ProductCategory.Battery)
            .ToList();
        BuildPart? battery = build.InCategory(ProductCategory.Battery).FirstOrDefault();
        int motorCount = build.MotorCount > 0 ? build.MotorCount : DefaultMotorCount;

        List<Guid> missing = installed.Where(p => p.WeightGrams is null).Select(p => p.ProductId).ToList();
        if (battery is { WeightGrams: null })
        {
            missing.Add(battery.ProductId);
        }

        decimal dry = installed
            .Where(p => p.WeightGrams is not null)
            .Sum(p => p.WeightGrams!.Value * (p.Category == ProductCategory.Propeller ? motorCount : p.Quantity));

        bool complete = missing.Count == 0 && battery is not null;
        decimal? allUp = complete ? dry + battery!.WeightGrams!.Value : null;

        decimal? thrust = build.With<MotorSpec>().Select(m => m.Spec.MaxThrustGrams).FirstOrDefault(t => t is not null);
        decimal? thrustToWeight = allUp is > 0 && thrust is not null
            ? Math.Round(motorCount * thrust.Value / allUp.Value, 1)
            : null;

        return new BuildWeight(Math.Round(dry, 1), allUp is null ? null : Math.Round(allUp.Value, 1), thrustToWeight,
            complete, missing);
    }
}
