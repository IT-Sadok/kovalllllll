using DroneBuilder.Application.Features.Images;
using DroneBuilder.Application.Features.Products.CreateProduct;
using DroneBuilder.Application.Features.Products.UpdateProduct;
using DroneBuilder.Domain.Entities;
namespace DroneBuilder.Application.Features.Products;

public static class ProductMappingExtensions
{
    public static ProductModel ToModel(this Product product)
    {
        if (product == null)
        {
            return null!;
        }

        return new ProductModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Category = product.Category,
            Manufacturer = product.Manufacturer,
            WeightGrams = product.WeightGrams,
            Attributes = product.Attributes?
                .OrderBy(a => a.SortOrder)
                .Select(a => new ProductAttributeModel(a.Name, a.Value))
                .ToList() ?? new List<ProductAttributeModel>(),
            Spec = product.Spec?.ToModel(),
            Images = product.Images != null ? product.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ToModel()).ToList() : new List<ImageModel>()
        };
    }

    public static Product ToEntity(this CreateProductModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new Product
        {
            Name = model.Name,
            Price = model.Price,
            Category = model.Category!.Value,
            Manufacturer = string.IsNullOrWhiteSpace(model.Manufacturer) ? null : model.Manufacturer.Trim(),
            WeightGrams = model.WeightGrams
        };
    }

    public static void UpdateEntity(this UpdateProductRequestModel model, Product entity)
    {
        if (model == null || entity == null)
        {
            return;
        }

        if (model.Name != null)
        {
            entity.Name = model.Name;
        }

        if (model.Price.HasValue)
        {
            entity.Price = model.Price.Value;
        }

        if (model.Category.HasValue)
        {
            entity.Category = model.Category.Value;
        }

        if (model.Manufacturer != null)
        {
            entity.Manufacturer = string.IsNullOrWhiteSpace(model.Manufacturer) ? null : model.Manufacturer.Trim();
        }

        if (model.WeightGrams.HasValue)
        {
            entity.WeightGrams = model.WeightGrams.Value > 0 ? model.WeightGrams.Value : null;
        }
    }
}
