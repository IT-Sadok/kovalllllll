using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class MotorKvRule : ICompatibilityRule
{
    private const decimal MinLoad = 30000m;
    private const decimal MaxLoad = 65000m;

    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        foreach (SpecPart<MotorSpec> motor in build.With<MotorSpec>())
        {
            foreach (SpecPart<BatterySpec> battery in build.With<BatterySpec>())
            {
                foreach (SpecPart<PropellerSpec> prop in build.With<PropellerSpec>())
                {
                    decimal load = motor.Spec.Kv * battery.Spec.Cells * prop.Spec.DiameterInch;
                    string setup = FormattableString.Invariant($"{motor.Spec.Kv} KV on {battery.Spec.Cells}S with {prop.Spec.DiameterInch:0.#}\" props");

                    if (load > MaxLoad)
                    {
                        yield return new CompatibilityIssue(IssueSeverity.Warning, "kv_too_high",
                            $"{setup} will draw a lot of current and run hot; use a lower KV motor or fewer cells.",
                            [motor.Id, battery.Id, prop.Id]);
                    }
                    else if (load < MinLoad)
                    {
                        yield return new CompatibilityIssue(IssueSeverity.Warning, "kv_too_low",
                            $"{setup} will feel underpowered; use a higher KV motor or more cells.",
                            [motor.Id, battery.Id, prop.Id]);
                    }
                }
            }
        }
    }
}
