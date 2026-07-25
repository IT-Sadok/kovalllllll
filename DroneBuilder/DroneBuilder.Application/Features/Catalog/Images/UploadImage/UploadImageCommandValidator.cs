using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Images.UploadImage;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    public UploadImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
        RuleFor(x => x.File).NotNull().WithMessage("File is required.");
        When(command => command.File is not null, () =>
        {
            RuleFor(command => command.File.Length)
                .InclusiveBetween(1, 10 * 1024 * 1024)
                .WithMessage("Image size must be between 1 byte and 10 MB.");
            RuleFor(command => command.File.ContentType)
                .Must(contentType => new[] { "image/jpeg", "image/png", "image/webp" }
                    .Contains(contentType, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Only JPEG, PNG and WebP images are supported.");
            RuleFor(command => Path.GetExtension(command.File.FileName))
                .Must(extension => new[] { ".jpg", ".jpeg", ".png", ".webp" }
                    .Contains(extension, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Unsupported image file extension.");
        });
    }
}

