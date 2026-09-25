using DroneBuilder.Application.Features.Builds.Compatibility.Rules;

namespace DroneBuilder.Application.Features.Builds.Compatibility;

public static class CompatibilityChecker
{
    private static readonly ICompatibilityRule[] Rules =
    [
        new RequiredPartsRule(),
        new MissingSpecRule(),
        new PropellerFitsFrameRule(),
        new StackMountRule(),
        new MotorMountRule(),
        new BatteryCellsRule(),
        new VideoSystemRule(),
        new CameraWidthRule(),
        new EscCurrentRule(),
        new BatteryConnectorRule(),
        new AntennaConnectorRule(),
        new PropellerHubRule(),
        new MotorKvRule(),
        new WeightRule()
    ];

    public static IReadOnlyList<CompatibilityIssue> Check(BuildParts build)
        => Rules.SelectMany(rule => rule.Check(build))
            .OrderBy(issue => issue.Severity)
            .ToList();
}
