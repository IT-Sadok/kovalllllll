using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public sealed class CatalogImportRepository(ApplicationDbContext dbContext) : ICatalogImportRepository
{
    public async Task<ICollection<ImportSource>> GetSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportSources
            .AsNoTracking()
            .OrderBy(source => source.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<ImportSource?> GetSourceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.ImportSources.FirstOrDefaultAsync(source => source.Id == id, cancellationToken);
    }

    public Task<bool> IsSourceCodeInUseAsync(
        string code,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ImportSources.AnyAsync(
            source => source.Code == code && (!excludedId.HasValue || source.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task AddSourceAsync(ImportSource source, CancellationToken cancellationToken = default)
    {
        await dbContext.ImportSources.AddAsync(source, cancellationToken);
    }

    public async Task<ICollection<ImportBatch>> GetBatchesAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportBatches
            .AsNoTracking()
            .Where(batch => batch.ImportSourceId == sourceId)
            .OrderByDescending(batch => batch.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ImportBatch?> GetBatchAsync(
        Guid sourceId,
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ImportBatches
            .AsNoTracking()
            .Include(batch => batch.Items)
            .FirstOrDefaultAsync(
                batch => batch.ImportSourceId == sourceId && batch.Id == batchId,
                cancellationToken);
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .AsSplitQuery()
            .Include(product => product.ExternalReferences)
            .Include(product => product.Variants)
                .ThenInclude(variant => variant.ExternalReferences)
            .FirstOrDefaultAsync(product => product.Id == id && product.IsActive, cancellationToken);
    }

    public Task<Property?> GetPropertyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Properties
            .Include(property => property.Aliases)
            .FirstOrDefaultAsync(property => property.Id == id, cancellationToken);
    }

    public Task<Value?> GetValueAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Values
            .Include(value => value.Aliases)
            .FirstOrDefaultAsync(value => value.Id == id, cancellationToken);
    }

    public Task<ProductExternalReference?> GetProductReferenceAsync(
        Guid sourceId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProductExternalReferences.FirstOrDefaultAsync(
            reference => reference.ImportSourceId == sourceId && reference.ExternalId == externalId,
            cancellationToken);
    }

    public Task<ProductVariantExternalReference?> GetVariantReferenceAsync(
        Guid sourceId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProductVariantExternalReferences.FirstOrDefaultAsync(
            reference => reference.ImportSourceId == sourceId && reference.ExternalId == externalId,
            cancellationToken);
    }

    public async Task AddProductReferenceAsync(
        ProductExternalReference reference,
        CancellationToken cancellationToken = default)
    {
        await dbContext.ProductExternalReferences.AddAsync(reference, cancellationToken);
    }

    public async Task AddVariantReferenceAsync(
        ProductVariantExternalReference reference,
        CancellationToken cancellationToken = default)
    {
        await dbContext.ProductVariantExternalReferences.AddAsync(reference, cancellationToken);
    }

    public void RemoveProductReference(ProductExternalReference reference)
    {
        dbContext.ProductExternalReferences.Remove(reference);
    }

    public void RemoveVariantReference(ProductVariantExternalReference reference)
    {
        dbContext.ProductVariantExternalReferences.Remove(reference);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
