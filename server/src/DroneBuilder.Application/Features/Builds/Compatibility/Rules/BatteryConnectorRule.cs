using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class BatteryConnectorRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        List<(Guid Id, string Name, BatteryConnector? Connector)> leads =
        [
            .. build.With<EscSpec>().Select(p => (p.Id, p.Name, p.Spec.BatteryConnector)),
            .. build.With<StackSpec>().Select(p => (p.Id, p.Name, p.Spec.BatteryConnector))
        ];

        foreach (SpecPart<BatterySpec> battery in build.With<BatterySpec>())
        {
            foreach ((Guid id, string name, BatteryConnector? connector) in leads)
            {
                if (connector is null)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Info, "battery_connector_unknown",
                        $"The battery lead of {name} is unknown, so the {SpecLabels.Of(battery.Spec.Connector)} plug on {battery.Name} could not be checked.",
                        [id, battery.Id]);
                }
                else if (connector != battery.Spec.Connector)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Warning, "battery_connector",
                        $"{battery.Name} has a {SpecLabels.Of(battery.Spec.Connector)} plug, but {name} has a {SpecLabels.Of(connector.Value)} lead; you will need an adapter or a new pigtail.",
                        [battery.Id, id]);
                }
            }
        }
    }
}
