namespace DroneBuilder.Application.Features.Catalog.Imports.Models;

public sealed class ImportSourceModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? BaseUrl { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateImportSourceModel
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? BaseUrl { get; init; }
}

public sealed class UpdateImportSourceModel
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? BaseUrl { get; init; }
    public bool ClearBaseUrl { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class ImportBatchModel
{
    public Guid Id { get; init; }
    public Guid ImportSourceId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? StartedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
    public int TotalItems { get; init; }
    public int ProcessedItems { get; init; }
    public int FailedItems { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class ImportItemModel
{
    public Guid Id { get; init; }
    public Guid ImportBatchId { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public string? ContentHash { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Error { get; init; }
    public Guid? ProductId { get; init; }
}

public sealed class ExternalReferenceModel
{
    public Guid Id { get; init; }
    public Guid ImportSourceId { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? ProductVariantId { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public string? SourceUrl { get; init; }
    public string? ContentHash { get; init; }
    public DateTime LastSeenAt { get; init; }
}

public sealed class UpsertExternalReferenceModel
{
    public string ExternalId { get; init; } = string.Empty;
    public string? SourceUrl { get; init; }
    public string? ContentHash { get; init; }
}

public sealed class ReplaceSourceAliasesModel
{
    public ICollection<string> Aliases { get; init; } = [];
}
