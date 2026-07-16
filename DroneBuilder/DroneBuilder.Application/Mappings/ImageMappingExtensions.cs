using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class ImageMappingExtensions
{
    public static ImageModel ToModel(this Image image)
    {
        if (image == null) return null!;
        return new ImageModel
        {
            Id = image.Id,
            Url = image.Url,
            IsPrimary = image.IsPrimary,
            FileName = image.FileName,
            UploadedAt = image.UploadedAt
        };
    }

    public static Image ToEntity(this UploadImageModel model)
    {
        if (model == null) return null!;
        return new Image
        {
            ProductId = model.ProductId,
            UploadedAt = DateTime.UtcNow
        };
    }

    public static ProductImagesResponseModel ToImagesResponseModel(this Product product)
    {
        if (product == null) return null!;
        return new ProductImagesResponseModel
        {
            Id = product.Id,
            Name = product.Name,
            Images = product.Images?.Select(i => i.ToModel()).ToList() ?? new List<ImageModel>(),
            Category = product.Category
        };
    }
}
