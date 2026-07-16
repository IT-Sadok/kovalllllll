using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

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
            Properties = product.ProductPropertyValues?
                .GroupBy(ppv => ppv.Property.Id)
                .Select(g => new PropertyModel
                {
                    Id = g.Key,
                    Name = g.First().Property.Name,
                    Values = g.Select(ppv => new ValueModel
                    {
                        Id = ppv.Value.Id,
                        Text = ppv.Value.Text
                    }).ToList()
                }).ToList() ?? new List<PropertyModel>(),
            Images = product.Images != null ? product.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ToModel()).ToList() : new List<ImageModel>()
        };
    }

    public static ProductPropertiesResponseModel ToPropertiesResponseModel(this Product product)
    {
        if (product == null)
        {
            return null!;
        }

        return new ProductPropertiesResponseModel
        {
            Id = product.Id,
            Name = product.Name,
            Properties = product.ProductPropertyValues?
                .GroupBy(ppv => ppv.Property.Id)
                .Select(g => new PropertyModel
                {
                    Id = g.Key,
                    Name = g.First().Property.Name,
                    Values = g.Select(ppv => new ValueModel
                    {
                        Id = ppv.Value.Id,
                        Text = ppv.Value.Text
                    }).ToList()
                }).ToList() ?? new List<PropertyModel>(),
            Images = product.Images?.Select(i => i.ToModel()).ToList() ?? new List<ImageModel>()
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
            Category = model.Category
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

        if (model.Category != null)
        {
            entity.Category = model.Category;
        }
    }
}
