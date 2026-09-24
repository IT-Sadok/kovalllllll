using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ProductEvents;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Application.Features.Imports.RaceDayQuads;

public interface IRaceDayQuadsImporter
{
    Task RunAsync(Guid importRunId, CancellationToken cancellationToken);
}

public class RaceDayQuadsImporter(
    IRaceDayQuadsClient client,
    IImportRunRepository importRunRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IUnitOfWork unitOfWork,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    RaceDayQuadsImportOptions options,
    ILogger<RaceDayQuadsImporter> logger) : IRaceDayQuadsImporter
{
    private const int MaxErrorLength = 2000;

    public async Task RunAsync(Guid importRunId, CancellationToken cancellationToken)
    {
        ImportRun? run = await importRunRepository.GetByIdAsync(importRunId, cancellationToken);
        if (run is null || run.Status != ImportRunStatus.Queued)
        {
            return;
        }

        run.Status = ImportRunStatus.Running;
        run.StartedAt = DateTime.UtcNow;
        await importRunRepository.SaveChangesAsync(cancellationToken);

        var counters = new Counters();
        ImportRunStatus status = ImportRunStatus.Succeeded;
        string? error = null;

        try
        {
            Warehouse warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken)
                                  ?? throw new InvalidOperationException("Warehouse not found.");

            foreach (RaceDayQuadsCategorySource source in RaceDayQuadsCatalog.Categories)
            {
                await ImportCategoryAsync(source, warehouse.Id, counters, cancellationToken);
                await SaveProgressAsync(importRunId, counters, null, null, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "RaceDayQuads import {ImportRunId} failed", importRunId);
            status = ImportRunStatus.Failed;
            error = ex.Message.Length > MaxErrorLength ? ex.Message[..MaxErrorLength] : ex.Message;
        }

        unitOfWork.ClearChanges();
        await SaveProgressAsync(importRunId, counters, status, error, CancellationToken.None);
    }

    private async Task ImportCategoryAsync(RaceDayQuadsCategorySource source, Guid warehouseId, Counters counters,
        CancellationToken cancellationToken)
    {
        int perCollection = (int)Math.Ceiling(options.MaxProductsPerCategory / (double)source.Collections.Count);
        int pageSize = Math.Min(250, perCollection * 2);

        foreach (RaceDayQuadsCollection collection in source.Collections)
        {
            int taken = 0;
            for (int page = 1; taken < perCollection; page++)
            {
                IReadOnlyList<ShopifyProduct> products =
                    await client.GetCollectionProductsAsync(collection.Handle, page, pageSize, cancellationToken);

                foreach (ShopifyProduct product in products.Where(p => !RaceDayQuadsCatalog.IsExcluded(p, source)))
                {
                    if (taken == perCollection)
                    {
                        break;
                    }

                    taken++;
                    await ImportProductAsync(product, source, collection, warehouseId, counters, cancellationToken);
                }

                if (products.Count < pageSize)
                {
                    break;
                }
            }
        }
    }

    private async Task ImportProductAsync(ShopifyProduct product, RaceDayQuadsCategorySource source,
        RaceDayQuadsCollection collection, Guid warehouseId, Counters counters, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<ImportedProduct> items = RaceDayQuadsProductParser.Parse(product, source.Category, collection,
                options.MaxVariantsPerProduct, options.BaseUrl);
            if (items.Count == 0)
            {
                return;
            }

            await using ITransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            Dictionary<string, Product> existing = (await productRepository.GetByExternalIdsAsync(
                    RaceDayQuadsCatalog.Source, items.Select(i => i.ExternalId).ToList(), cancellationToken))
                .ToDictionary(p => p.ExternalId!);

            ProductGroup? group = items.Count > 1
                ? await GetOrCreateGroupAsync(product, cancellationToken)
                : null;

            var result = new Counters();
            foreach (ImportedProduct item in items)
            {
                if (existing.TryGetValue(item.ExternalId, out Product? current))
                {
                    if (current.IsDeleted)
                    {
                        result.Skipped++;
                        continue;
                    }

                    await ApplyAsync(current, item, group, cancellationToken);
                    result.Updated++;
                    result.NeedsReview += current.NeedsReview ? 1 : 0;
                    continue;
                }

                var created = new Product { Category = source.Category };
                await ApplyAsync(created, item, group, cancellationToken);
                created.Images = item.ImageUrls.Select((url, index) => new Image
                {
                    Url = url,
                    FileName = Path.GetFileName(new Uri(url).AbsolutePath),
                    UploadedAt = DateTime.UtcNow,
                    IsPrimary = index == 0
                }).ToList();

                await productRepository.AddProductAsync(created, cancellationToken);
                await warehouseRepository.AddWarehouseItemAsync(
                    new WarehouseItem { WarehouseId = warehouseId, ProductId = created.Id }, cancellationToken);
                await outboxService.StoreEventAsync(new ProductCreatedEvent(created.Id), queuesConfig.ProductQueue.Name,
                    cancellationToken);

                result.Added++;
                result.NeedsReview += created.NeedsReview ? 1 : 0;
            }

            await productRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            counters.Add(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to import RaceDayQuads product {ShopifyProductId} ({Title})", product.Id,
                product.Title);
            unitOfWork.ClearChanges();
            counters.Failed++;
        }
    }

    private async Task<ProductGroup> GetOrCreateGroupAsync(ShopifyProduct product, CancellationToken cancellationToken)
    {
        string externalId = product.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string name = product.Title.Length > 300 ? product.Title[..300] : product.Title;

        ProductGroup? group =
            await productRepository.GetGroupByExternalIdAsync(RaceDayQuadsCatalog.Source, externalId, cancellationToken);
        if (group is not null)
        {
            group.Name = name;
            return group;
        }

        group = new ProductGroup { Name = name, ExternalSource = RaceDayQuadsCatalog.Source, ExternalId = externalId };
        await productRepository.AddGroupAsync(group, cancellationToken);
        return group;
    }

    private async Task ApplyAsync(Product product, ImportedProduct item, ProductGroup? group,
        CancellationToken cancellationToken)
    {
        product.Name = item.Name;
        product.Price = item.Price;
        product.Manufacturer = item.Manufacturer ?? product.Manufacturer;
        product.WeightGrams = item.WeightGrams ?? product.WeightGrams;
        product.VariantName = item.VariantName;
        product.ExternalSource = RaceDayQuadsCatalog.Source;
        product.ExternalId = item.ExternalId;
        product.SourceUrl = item.SourceUrl;
        product.Group = group;

        product.Attributes.Clear();
        for (int i = 0; i < item.Attributes.Count; i++)
        {
            product.Attributes.Add(new ProductAttribute
            {
                Name = item.Attributes[i].Name,
                Value = item.Attributes[i].Value,
                SortOrder = i
            });
        }

        if (item.Spec is not null && product.Category.ToComponentType() == item.Spec.Type)
        {
            if (product.Spec is not null)
            {
                product.Spec = null;
                await productRepository.SaveChangesAsync(cancellationToken);
            }

            item.Spec.ProductId = product.Id;
            product.Spec = item.Spec;
        }

        product.NeedsReview = product.Category.ToComponentType() is not null && product.Spec is null;
    }

    private async Task SaveProgressAsync(Guid importRunId, Counters counters, ImportRunStatus? status, string? error,
        CancellationToken cancellationToken)
    {
        ImportRun? run = await importRunRepository.GetByIdAsync(importRunId, cancellationToken);
        if (run is null)
        {
            return;
        }

        run.Added = counters.Added;
        run.Updated = counters.Updated;
        run.Skipped = counters.Skipped;
        run.NeedsReview = counters.NeedsReview;
        run.Failed = counters.Failed;

        if (status.HasValue)
        {
            run.Status = status.Value;
            run.Error = error;
            run.FinishedAt = DateTime.UtcNow;
        }

        await importRunRepository.SaveChangesAsync(cancellationToken);
    }

    private sealed class Counters
    {
        public int Added { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public int NeedsReview { get; set; }
        public int Failed { get; set; }

        public void Add(Counters other)
        {
            Added += other.Added;
            Updated += other.Updated;
            Skipped += other.Skipped;
            NeedsReview += other.NeedsReview;
            Failed += other.Failed;
        }
    }
}
