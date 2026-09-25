using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Builds.Compatibility;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Builds.CheckBuild;

public class CheckBuildQueryHandler(IProductRepository productRepository)
    : IQueryHandler<CheckBuildQuery, BuildCheckModel>
{
    public async Task<Result<BuildCheckModel>> ExecuteAsync(CheckBuildQuery query, CancellationToken cancellationToken)
    {
        List<Guid> ids = query.Items.Select(i => i.ProductId).ToList();
        Dictionary<Guid, Product> products =
            (await productRepository.GetProductsWithSpecsByIdsAsync(ids, cancellationToken)).ToDictionary(p => p.Id);

        List<Guid> unknown = ids.Where(id => !products.ContainsKey(id)).ToList();
        if (unknown.Count > 0)
        {
            return Result.Fail<BuildCheckModel>(new NotFoundError($"Products not found: {string.Join(", ", unknown)}."));
        }

        var build = new BuildParts(query.Items.Select(item =>
        {
            Product product = products[item.ProductId];
            return new BuildPart(product.Id, product.Name, product.Category, product.Spec, product.Price,
                product.WeightGrams, item.Quantity);
        }).ToList());

        IReadOnlyList<CompatibilityIssue> issues = CompatibilityChecker.Check(build);

        return Result.Ok(new BuildCheckModel(
            IsValid: issues.All(i => i.Severity != IssueSeverity.Error),
            TotalPrice: build.All.Sum(p => p.Price * p.Quantity),
            Weight: BuildWeightCalculator.Calculate(build),
            Issues: issues));
    }
}

public record CheckBuildQuery(List<BuildItemModel> Items);

public record BuildItemModel(Guid ProductId, int Quantity);

public record BuildCheckModel(bool IsValid, decimal TotalPrice, BuildWeight Weight, IReadOnlyList<CompatibilityIssue> Issues);
