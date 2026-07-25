using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class ImageMappingExtensions
{
    public static ImageModel ToModel(this Image image)
    {
        return new ImageModel
        {
            Id = image.Id,
            Url = image.Url,
            IsPrimary = image.IsPrimary,
            FileName = image.FileName,
            UploadedAt = image.UploadedAt
        };
    }
}
