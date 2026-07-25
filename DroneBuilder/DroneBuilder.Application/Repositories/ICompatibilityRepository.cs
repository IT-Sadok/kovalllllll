using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface ICompatibilityRepository
{
    Task<ICollection<CompatibilityRule>> GetRulesAsync(CancellationToken cancellationToken = default);
    Task<CompatibilityRule?> GetRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<CompatibilityRule>> GetApplicableRulesAsync(Guid leftComponentTypeId, Guid rightComponentTypeId, CancellationToken cancellationToken = default);
    Task<bool> IsCodeInUseAsync(string code, Guid? excludedId = null, CancellationToken cancellationToken = default);
    Task<ProductVariant?> GetVariantAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddRuleAsync(CompatibilityRule rule, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
