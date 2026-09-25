using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class PropellerHubRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        foreach (SpecPart<PropellerSpec> prop in build.With<PropellerSpec>())
        {
            foreach (SpecPart<MotorSpec> motor in build.With<MotorSpec>())
            {
                if (prop.Spec.HubMm is not { } hub || motor.Spec.ShaftMm is not { } shaft)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Info, "prop_hub_unknown",
                        $"The hub of {prop.Name} or the shaft of {motor.Name} is unknown, so the prop fit could not be checked.",
                        [prop.Id, motor.Id]);
                }
                else if (hub != shaft)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Error, "prop_hub",
                        FormattableString.Invariant($"{prop.Name} has a {hub:0.#} mm hub, but {motor.Name} has a {shaft:0.#} mm shaft."),
                        [prop.Id, motor.Id]);
                }
            }
        }
    }
}
