using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class VideoSystemRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        foreach (SpecPart<CameraSpec> camera in build.With<CameraSpec>())
        {
            foreach (SpecPart<VideoTransmitterSpec> vtx in build.With<VideoTransmitterSpec>())
            {
                if (camera.Spec.VideoSystem != vtx.Spec.VideoSystem)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Error, "video_system",
                        $"{camera.Name} is {SpecLabels.Of(camera.Spec.VideoSystem)}, but {vtx.Name} is {SpecLabels.Of(vtx.Spec.VideoSystem)}.",
                        [camera.Id, vtx.Id]);
                }
            }
        }
    }
}
