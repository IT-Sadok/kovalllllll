using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class MotorMountRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        foreach (SpecPart<FrameSpec> frame in build.With<FrameSpec>())
        {
            foreach (SpecPart<MotorSpec> motor in build.With<MotorSpec>())
            {
                if (frame.Spec.MotorMountPatterns.Count == 0)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Info, "motor_mount_unknown",
                        $"The motor mounting of {frame.Name} is unknown, so {motor.Name} could not be checked.",
                        [frame.Id, motor.Id]);
                }
                else if (!frame.Spec.MotorMountPatterns.Contains(motor.Spec.MountPattern))
                {
                    yield return new CompatibilityIssue(IssueSeverity.Error, "motor_mount",
                        $"{motor.Name} bolts on {SpecLabels.Of(motor.Spec.MountPattern)}, but {frame.Name} takes {SpecLabels.Of(frame.Spec.MotorMountPatterns)}.",
                        [motor.Id, frame.Id]);
                }
            }
        }
    }
}
