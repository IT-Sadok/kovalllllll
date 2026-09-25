using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class MissingSpecRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
        => build.All
            .Where(p => p.Spec is null && p.Category.ToComponentType() is not null)
            .Select(p => new CompatibilityIssue(IssueSeverity.Info, "missing_spec",
                $"{p.Name} has no specification yet, so it could not be checked.", [p.ProductId]));
}
