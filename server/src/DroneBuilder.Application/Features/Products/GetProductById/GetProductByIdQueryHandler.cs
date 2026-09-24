using System.Globalization;
using System.Text.RegularExpressions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Products.GetProductById;

public partial class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetProductByIdQuery, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Fail<ProductModel>(new NotFoundError($"Product with id {query.ProductId} not found."));
        }

        ProductModel model = product.ToModel();

        WarehouseItem? warehouseItem = await warehouseRepository.GetWarehouseItemByProductIdAsync(product.Id, cancellationToken);
        if (warehouseItem != null)
        {
            model.StockQuantity = warehouseItem.Quantity;
        }

        if (product.GroupId.HasValue)
        {
            model.Group = await BuildGroupAsync(product.GroupId.Value, cancellationToken);
        }

        return Result.Ok(model);
    }

    private async Task<ProductGroupModel?> BuildGroupAsync(Guid groupId, CancellationToken cancellationToken)
    {
        ICollection<Product> variants = await productRepository.GetGroupVariantsAsync(groupId, cancellationToken);
        ProductGroupSummary? summary =
            (await productRepository.GetGroupSummariesAsync([groupId], cancellationToken)).FirstOrDefault();
        if (summary is null)
        {
            return null;
        }

        Dictionary<Guid, int> stock = (await warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                variants.Select(v => v.Id).ToList(), cancellationToken))
            .GroupBy(w => w.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(w => w.Quantity));

        ProductGroupModel group = summary.ToModel();
        group.Variants = variants
            .OrderBy(v => LeadingNumber(v.VariantName))
            .ThenBy(v => v.VariantName, StringComparer.OrdinalIgnoreCase)
            .Select(v => new ProductVariantModel(v.Id, v.VariantName, v.Price, stock.GetValueOrDefault(v.Id)))
            .ToList();
        return group;
    }

    private static decimal LeadingNumber(string? name)
        => name is not null && NumberRegex().Match(name) is { Success: true } match
            ? decimal.Parse(match.Value, CultureInfo.InvariantCulture)
            : decimal.MaxValue;

    [GeneratedRegex(@"\d+(?:\.\d+)?")]
    private static partial Regex NumberRegex();
}

public record GetProductByIdQuery(Guid ProductId);
