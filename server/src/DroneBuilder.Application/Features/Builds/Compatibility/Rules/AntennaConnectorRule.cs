using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class AntennaConnectorRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        foreach (SpecPart<VideoTransmitterSpec> vtx in build.With<VideoTransmitterSpec>())
        {
            foreach (SpecPart<AntennaSpec> antenna in build.With<AntennaSpec>())
            {
                if (vtx.Spec.AntennaConnector is not { } vtxConnector)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Info, "antenna_connector_unknown",
                        $"The antenna connector of {vtx.Name} is unknown, so {antenna.Name} could not be checked.",
                        [vtx.Id, antenna.Id]);
                }
                else if (vtxConnector != antenna.Spec.Connector)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Warning, "antenna_connector",
                        $"{antenna.Name} has a {SpecLabels.Of(antenna.Spec.Connector)} connector, but {vtx.Name} expects {SpecLabels.Of(vtxConnector)}; an adapter is needed.",
                        [antenna.Id, vtx.Id]);
                }
            }
        }
    }
}
