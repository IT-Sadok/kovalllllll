using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class CameraWidthRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        foreach (SpecPart<FrameSpec> frame in build.With<FrameSpec>())
        {
            foreach (SpecPart<CameraSpec> camera in build.With<CameraSpec>())
            {
                if (frame.Spec.CameraWidthMm is not { } frameWidth || camera.Spec.WidthMm is not { } cameraWidth)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Info, "camera_width_unknown",
                        $"The camera size of {frame.Name} or {camera.Name} is unknown, so the camera fit could not be checked.",
                        [frame.Id, camera.Id]);
                }
                else if (frameWidth != cameraWidth)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Warning, "camera_width",
                        $"{camera.Name} is {cameraWidth} mm wide, but {frame.Name} holds {frameWidth} mm cameras; an adapter may be needed.",
                        [camera.Id, frame.Id]);
                }
            }
        }
    }
}
