namespace DroneBuilder.Domain.Entities;

public class ImportRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Source { get; set; } = string.Empty;
    public ImportRunStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int Added { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public int NeedsReview { get; set; }
    public int Failed { get; set; }
    public string? Error { get; set; }
}
