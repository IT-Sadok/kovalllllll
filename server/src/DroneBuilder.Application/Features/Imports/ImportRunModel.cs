using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Imports;

public record ImportRunModel(
    Guid Id,
    string Source,
    ImportRunStatus Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    int Added,
    int Updated,
    int Skipped,
    int NeedsReview,
    int Failed,
    string? Error);

public static class ImportRunMappingExtensions
{
    public static ImportRunModel ToModel(this ImportRun run) => new(
        run.Id, run.Source, run.Status, run.CreatedAt, run.StartedAt, run.FinishedAt,
        run.Added, run.Updated, run.Skipped, run.NeedsReview, run.Failed, run.Error);
}
