using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Images;

public static class ImageMappingExtensions
{
    public static ImageModel ToModel(this Image image)
    {
        if (image == null)
        {
            return null!;
        }

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
