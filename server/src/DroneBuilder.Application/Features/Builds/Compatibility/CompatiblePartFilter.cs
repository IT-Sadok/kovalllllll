namespace DroneBuilder.Application.Features.Builds.Compatibility;

public static class CompatiblePartFilter
{
    public static IReadOnlyList<Guid> Filter(IReadOnlyList<BuildPart> selected, IEnumerable<BuildPart> candidates)
        => candidates
            .Where(candidate => candidate.Spec is not null && !HasErrorWith(selected, candidate))
            .Select(candidate => candidate.ProductId)
            .ToList();

    private static bool HasErrorWith(IReadOnlyList<BuildPart> selected, BuildPart candidate)
    {
        var build = new BuildParts([.. selected.Where(p => p.ProductId != candidate.ProductId), candidate]);

        return CompatibilityChecker.Check(build)
            .Any(issue => issue.Severity == IssueSeverity.Error && issue.ProductIds.Contains(candidate.ProductId));
    }
}
