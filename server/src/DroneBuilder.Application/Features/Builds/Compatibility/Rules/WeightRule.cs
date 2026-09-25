namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class WeightRule : ICompatibilityRule
{
    private const decimal RegulatoryLimitGrams = 250m;

    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        BuildWeight weight = BuildWeightCalculator.Calculate(build);

        if (weight.MissingWeightProductIds.Count > 0)
        {
            string names = string.Join(", ", build.All
                .Where(p => weight.MissingWeightProductIds.Contains(p.ProductId))
                .Select(p => p.Name));
            yield return new CompatibilityIssue(IssueSeverity.Info, "weight_unknown",
                $"The weight of {names} is unknown, so the take-off weight is incomplete.", weight.MissingWeightProductIds);
        }

        if (weight.AllUpGrams > RegulatoryLimitGrams)
        {
            yield return new CompatibilityIssue(IssueSeverity.Info, "over_250g",
                FormattableString.Invariant($"Take-off weight is about {weight.AllUpGrams:0} g, above the 250 g limit that many countries use for lighter drone rules."),
                []);
        }
    }
}
