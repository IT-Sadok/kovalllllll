using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class RequiredPartsRule : ICompatibilityRule
{
    private const int QuadMotorCount = 4;

    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        IReadOnlyList<BuildPart> frames = build.InCategory(ProductCategory.Frame);
        IReadOnlyList<BuildPart> motors = build.InCategory(ProductCategory.Motor);
        bool hasStack = build.InCategory(ProductCategory.Stack).Count > 0;
        bool hasFc = build.InCategory(ProductCategory.FlightController).Count > 0;
        bool hasEsc = build.InCategory(ProductCategory.Esc).Count > 0;

        if (frames.Count == 0)
        {
            yield return Error("missing_frame", "Add a frame.");
        }
        else if (frames.Count > 1)
        {
            yield return new CompatibilityIssue(IssueSeverity.Warning, "multiple_frames",
                "The build has more than one frame.", frames.Select(f => f.ProductId).ToList());
        }

        if (motors.Count == 0)
        {
            yield return Error("missing_motors", $"Add {QuadMotorCount} motors.");
        }
        else
        {
            if (build.MotorCount != QuadMotorCount)
            {
                yield return new CompatibilityIssue(IssueSeverity.Error, "motor_count",
                    $"A quadcopter needs {QuadMotorCount} motors, the build has {build.MotorCount}.",
                    motors.Select(m => m.ProductId).ToList());
            }

            if (motors.Count > 1)
            {
                yield return new CompatibilityIssue(IssueSeverity.Warning, "mixed_motors",
                    "Use four identical motors so the quad flies evenly.", motors.Select(m => m.ProductId).ToList());
            }
        }

        if (build.InCategory(ProductCategory.Propeller).Count == 0)
        {
            yield return Error("missing_propellers", "Add propellers.");
        }

        if (hasStack && (hasFc || hasEsc))
        {
            yield return new CompatibilityIssue(IssueSeverity.Warning, "stack_duplicates_fc_esc",
                "The stack already includes a flight controller and an ESC.",
                build.All.Where(p => p.Category is ProductCategory.FlightController or ProductCategory.Esc)
                    .Select(p => p.ProductId).ToList());
        }
        else if (!hasStack && !hasFc && !hasEsc)
        {
            yield return Error("missing_fc_esc", "Add a flight controller and an ESC, or a stack that combines both.");
        }
        else if (!hasStack && !hasFc)
        {
            yield return Error("missing_fc", "Add a flight controller.");
        }
        else if (!hasStack && !hasEsc)
        {
            yield return Error("missing_esc", "Add an ESC.");
        }

        if (build.InCategory(ProductCategory.Battery).Count == 0)
        {
            yield return Error("missing_battery", "Add a battery.");
        }
    }

    private static CompatibilityIssue Error(string code, string message)
        => new(IssueSeverity.Error, code, message, []);
}
