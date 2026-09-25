namespace DroneBuilder.Application.Features.Builds.Compatibility;

public interface ICompatibilityRule
{
    IEnumerable<CompatibilityIssue> Check(BuildParts build);
}
