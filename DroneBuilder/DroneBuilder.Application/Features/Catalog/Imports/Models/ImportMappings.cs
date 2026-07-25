using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Catalog.Imports.Models;

public static class ImportMappings
{
    public static ImportSourceModel ToModel(this ImportSource source) => new()
    {
        Id = source.Id,
        Code = source.Code,
        Name = source.Name,
        BaseUrl = source.BaseUrl,
        IsActive = source.IsActive
    };

    public static ImportBatchModel ToModel(this ImportBatch batch) => new()
    {
        Id = batch.Id,
        ImportSourceId = batch.ImportSourceId,
        Status = batch.Status.ToString(),
        StartedAt = batch.StartedAt,
        FinishedAt = batch.FinishedAt,
        TotalItems = batch.TotalItems,
        ProcessedItems = batch.ProcessedItems,
        FailedItems = batch.FailedItems,
        CreatedAt = batch.CreatedAt
    };

    public static ImportItemModel ToModel(this ImportItem item) => new()
    {
        Id = item.Id,
        ImportBatchId = item.ImportBatchId,
        ExternalId = item.ExternalId,
        ContentHash = item.ContentHash,
        Status = item.Status.ToString(),
        Error = item.Error,
        ProductId = item.ProductId
    };

    public static ExternalReferenceModel ToModel(this ProductExternalReference reference) => new()
    {
        Id = reference.Id,
        ImportSourceId = reference.ImportSourceId,
        ProductId = reference.ProductId,
        ExternalId = reference.ExternalId,
        SourceUrl = reference.SourceUrl,
        ContentHash = reference.ContentHash,
        LastSeenAt = reference.LastSeenAt
    };

    public static ExternalReferenceModel ToModel(this ProductVariantExternalReference reference) => new()
    {
        Id = reference.Id,
        ImportSourceId = reference.ImportSourceId,
        ProductVariantId = reference.ProductVariantId,
        ExternalId = reference.ExternalId,
        SourceUrl = reference.SourceUrl,
        ContentHash = reference.ContentHash,
        LastSeenAt = reference.LastSeenAt
    };
}
