using DroneBuilder.Application.Features.Catalog.Compatibility.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.GetCompatibilityRules;

public sealed class GetCompatibilityRulesQueryHandler(ICompatibilityRepository repository)
    : IQueryHandler<GetCompatibilityRulesQuery, ICollection<CompatibilityRuleModel>>
{
    public async Task<Result<ICollection<CompatibilityRuleModel>>> ExecuteAsync(
        GetCompatibilityRulesQuery query,
        CancellationToken cancellationToken)
    {
        return Result.Ok<ICollection<CompatibilityRuleModel>>(
            (await repository.GetRulesAsync(cancellationToken)).Select(rule => rule.ToModel()).ToList());
    }
}

public sealed record GetCompatibilityRulesQuery;
