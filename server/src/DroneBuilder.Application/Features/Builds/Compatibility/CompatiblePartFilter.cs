namespace DroneBuilder.Application.Features.Builds.Compatibility;

public static class CompatiblePartFilter
{
    private static readonly HashSet<string> HidingWarningCodes = ["prop_undersized"];

    public static IReadOnlyList<Guid> Filter(IReadOnlyList<BuildPart> selected, IEnumerable<BuildPart> candidates)
        => candidates
            .Where(candidate => candidate.Spec is not null && !DoesNotFit(selected, candidate))
            .Select(candidate => candidate.ProductId)
            .ToList();

    private static bool DoesNotFit(IReadOnlyList<BuildPart> selected, BuildPart candidate)
    {
        var build = new BuildParts([.. selected.Where(p => p.ProductId != candidate.ProductId), candidate]);

        return CompatibilityChecker.Check(build)
            .Any(issue => issue.ProductIds.Contains(candidate.ProductId) &&
                          (issue.Severity == IssueSeverity.Error || HidingWarningCodes.Contains(issue.Code)));
    }
}
