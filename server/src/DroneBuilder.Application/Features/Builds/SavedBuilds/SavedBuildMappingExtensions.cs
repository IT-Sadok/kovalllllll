using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds;

public static class SavedBuildMappingExtensions
{
    public static SavedBuildModel ToModel(this Build build)
    {
        List<SavedBuildItemModel> items = build.Items
            .Where(i => i.Product is not null)
            .Select(i => new SavedBuildItemModel(
                i.ProductId,
                i.Product!.Name,
                i.Product.Category,
                i.Product.Price,
                i.Quantity,
                (i.Product.Images.FirstOrDefault(img => img.IsPrimary) ?? i.Product.Images.FirstOrDefault())?.Url,
                !i.Product.IsDeleted))
            .OrderBy(i => i.Category)
            .ToList();

        return new SavedBuildModel(build.Id, build.Name, build.CreatedAt, build.UpdatedAt,
            items.Where(i => i.IsAvailable).Sum(i => i.Price * i.Quantity), items);
    }
}
