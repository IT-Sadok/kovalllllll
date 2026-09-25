using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class EscCurrentRule : ICompatibilityRule
{
    private const decimal SafetyMargin = 1.2m;

    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        List<(Guid Id, string Name, decimal? Current)> escs =
        [
            .. build.With<EscSpec>().Select(p => (p.Id, p.Name, p.Spec.ContinuousCurrentA)),
            .. build.With<StackSpec>().Select(p => (p.Id, p.Name, p.Spec.ContinuousCurrentA))
        ];

        foreach ((Guid escId, string escName, decimal? escCurrent) in escs)
        {
            foreach (SpecPart<MotorSpec> motor in build.With<MotorSpec>())
            {
                if (escCurrent is null || motor.Spec.MaxCurrentA is null)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Info, "esc_current_unknown",
                        $"The current rating of {escName} or {motor.Name} is unknown, so the ESC headroom could not be checked.",
                        [escId, motor.Id]);
                }
                else if (escCurrent < motor.Spec.MaxCurrentA * SafetyMargin)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Warning, "esc_current",
                        FormattableString.Invariant($"{escName} is rated {escCurrent:0.#} A, but {motor.Name} can pull {motor.Spec.MaxCurrentA:0.#} A; pick an ESC with about 20% more headroom."),
                        [escId, motor.Id]);
                }
            }
        }
    }
}
