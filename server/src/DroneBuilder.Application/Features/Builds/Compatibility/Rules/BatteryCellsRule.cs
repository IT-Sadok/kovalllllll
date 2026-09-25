using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class BatteryCellsRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        List<(Guid Id, string Name, int Min, int Max)> powered =
        [
            .. build.With<MotorSpec>().Select(p => (p.Id, p.Name, p.Spec.MinCells, p.Spec.MaxCells)),
            .. build.With<EscSpec>().Select(p => (p.Id, p.Name, p.Spec.MinCells, p.Spec.MaxCells)),
            .. build.With<StackSpec>().Select(p => (p.Id, p.Name, p.Spec.MinCells, p.Spec.MaxCells)),
            .. build.With<FlightControllerSpec>().Select(p => (p.Id, p.Name, p.Spec.MinCells, p.Spec.MaxCells))
        ];

        foreach (SpecPart<BatterySpec> battery in build.With<BatterySpec>())
        {
            int cells = battery.Spec.Cells;
            foreach ((Guid id, string name, int min, int max) in powered)
            {
                if (cells < min || cells > max)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Error, "battery_cells",
                        $"{battery.Name} is {cells}S, but {name} is rated for {SpecLabels.Cells(min, max)}.",
                        [battery.Id, id]);
                }
            }
        }
    }
}
