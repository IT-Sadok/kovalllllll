using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class PropellerFitsFrameRule : ICompatibilityRule
{
    private const decimal UndersizedMarginInch = 1m;

    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        foreach (SpecPart<FrameSpec> frame in build.With<FrameSpec>())
        {
            foreach (SpecPart<PropellerSpec> prop in build.With<PropellerSpec>())
            {
                decimal max = frame.Spec.MaxPropSizeInch;
                decimal size = prop.Spec.DiameterInch;

                if (size > max)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Error, "prop_too_large",
                        FormattableString.Invariant($"{prop.Name} ({size:0.#}\") is larger than the {max:0.#}\" props {frame.Name} takes."),
                        [prop.Id, frame.Id]);
                }
                else if (size < max - UndersizedMarginInch)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Warning, "prop_undersized",
                        FormattableString.Invariant($"{prop.Name} ({size:0.#}\") is much smaller than the {max:0.#}\" {frame.Name} is built for."),
                        [prop.Id, frame.Id]);
                }
            }
        }
    }
}
