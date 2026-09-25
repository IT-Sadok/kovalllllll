using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class StackMountRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        List<(Guid Id, string Name, MountPattern Mount)> boards =
        [
            .. build.With<FlightControllerSpec>().Select(p => (p.Id, p.Name, p.Spec.MountPattern)),
            .. build.With<EscSpec>().Select(p => (p.Id, p.Name, p.Spec.MountPattern)),
            .. build.With<StackSpec>().Select(p => (p.Id, p.Name, p.Spec.MountPattern))
        ];

        foreach (SpecPart<FrameSpec> frame in build.With<FrameSpec>())
        {
            foreach ((Guid id, string name, MountPattern mount) in boards)
            {
                if (!frame.Spec.FcMountPatterns.Contains(mount))
                {
                    yield return new CompatibilityIssue(IssueSeverity.Error, "stack_mount",
                        $"{name} mounts on {SpecLabels.Of(mount)}, but {frame.Name} takes {SpecLabels.Of(frame.Spec.FcMountPatterns)}.",
                        [id, frame.Id]);
                }
            }

            foreach (SpecPart<VideoTransmitterSpec> vtx in build.With<VideoTransmitterSpec>())
            {
                if (vtx.Spec.MountPattern is { } mount && !frame.Spec.FcMountPatterns.Contains(mount))
                {
                    yield return new CompatibilityIssue(IssueSeverity.Warning, "vtx_mount",
                        $"{vtx.Name} mounts on {SpecLabels.Of(mount)}, which {frame.Name} does not offer; it may need its own mount.",
                        [vtx.Id, frame.Id]);
                }
            }
        }

        foreach (SpecPart<FlightControllerSpec> fc in build.With<FlightControllerSpec>())
        {
            foreach (SpecPart<EscSpec> esc in build.With<EscSpec>())
            {
                if (fc.Spec.MountPattern != esc.Spec.MountPattern)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Warning, "fc_esc_mount_mismatch",
                        $"{fc.Name} ({SpecLabels.Of(fc.Spec.MountPattern)}) and {esc.Name} ({SpecLabels.Of(esc.Spec.MountPattern)}) will not stack together.",
                        [fc.Id, esc.Id]);
                }
            }
        }
    }
}
