using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public sealed class CompatibilityRepository(ApplicationDbContext dbContext) : ICompatibilityRepository
{
    public async Task<ICollection<CompatibilityRule>> GetRulesAsync(
        CancellationToken cancellationToken = default)
    {
        return await RuleQuery()
            .AsNoTracking()
            .OrderBy(rule => rule.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<CompatibilityRule?> GetRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return RuleQuery().FirstOrDefaultAsync(rule => rule.Id == id, cancellationToken);
    }

    public async Task<ICollection<CompatibilityRule>> GetApplicableRulesAsync(
        Guid leftComponentTypeId,
        Guid rightComponentTypeId,
        CancellationToken cancellationToken = default)
    {
        return await RuleQuery()
            .AsNoTracking()
            .Where(rule => rule.IsActive &&
                (rule.LeftComponentTypeId == leftComponentTypeId &&
                 rule.RightComponentTypeId == rightComponentTypeId ||
                 rule.LeftComponentTypeId == rightComponentTypeId &&
                 rule.RightComponentTypeId == leftComponentTypeId))
            .OrderBy(rule => rule.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsCodeInUseAsync(
        string code,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CompatibilityRules.AnyAsync(
            rule => rule.Code == code && (!excludedId.HasValue || rule.Id != excludedId.Value),
            cancellationToken);
    }

    public Task<ProductVariant?> GetVariantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.ProductVariants
            .AsNoTracking()
            .AsSplitQuery()
            .Include(variant => variant.Product)
                .ThenInclude(product => product!.ComponentType)
            .Include(variant => variant.Product)
                .ThenInclude(product => product!.ProductPropertyValues)
                    .ThenInclude(specification => specification.Property)
                        .ThenInclude(property => property!.UnitDefinition)
            .Include(variant => variant.Product)
                .ThenInclude(product => product!.ProductPropertyValues)
                    .ThenInclude(specification => specification.Value)
            .Include(variant => variant.Specifications)
                .ThenInclude(specification => specification.Property)
                    .ThenInclude(property => property!.UnitDefinition)
            .Include(variant => variant.Specifications)
                .ThenInclude(specification => specification.Value)
            .FirstOrDefaultAsync(
                variant => variant.Id == id && variant.IsActive &&
                           variant.Product!.IsActive &&
                           variant.Product.PublicationStatus == ProductPublicationStatus.Published,
                cancellationToken);
    }

    public async Task AddRuleAsync(CompatibilityRule rule, CancellationToken cancellationToken = default)
    {
        await dbContext.CompatibilityRules.AddAsync(rule, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<CompatibilityRule> RuleQuery()
    {
        return dbContext.CompatibilityRules
            .AsSplitQuery()
            .Include(rule => rule.LeftComponentType)
                .ThenInclude(type => type!.Properties)
            .Include(rule => rule.RightComponentType)
                .ThenInclude(type => type!.Properties)
            .Include(rule => rule.LeftProperty)
                .ThenInclude(property => property!.UnitDefinition)
            .Include(rule => rule.LeftProperty)
                .ThenInclude(property => property!.Values)
            .Include(rule => rule.RightProperty)
                .ThenInclude(property => property!.UnitDefinition)
            .Include(rule => rule.RightProperty)
                .ThenInclude(property => property!.Values);
    }
}
