using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DroneBuilder.Application.Features.Images.UploadImage;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];

    public UploadImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.File).NotNull().WithMessage("File is required.");

        When(x => x.File != null, () =>
        {
            RuleFor(x => x.File.Length)
                .GreaterThan(0).WithMessage("File is empty.")
                .LessThanOrEqualTo(MaxFileSizeInBytes)
                .WithMessage($"File must not exceed {MaxFileSizeInBytes / (1024 * 1024)} MB.");

            RuleFor(x => x.File.FileName)
                .Must(HasAllowedExtension)
                .WithMessage($"File must be one of: {string.Join(", ", AllowedExtensions)}.")
                .MaximumLength(200).WithMessage("File name must not exceed 200 characters.");

            RuleFor(x => x.File.ContentType)
                .Must(contentType => AllowedContentTypes.Contains(contentType))
                .WithMessage($"Content type must be one of: {string.Join(", ", AllowedContentTypes)}.");

            RuleFor(x => x.File)
                .Must(HasImageSignature)
                .WithMessage("File content is not a valid image.")
                .When(x => HasAllowedExtension(x.File.FileName));
        });
    }

    private static bool HasAllowedExtension(string? fileName)
        => !string.IsNullOrWhiteSpace(fileName)
        && AllowedExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);

    private static bool HasImageSignature(IFormFile file)
    {
        Span<byte> header = stackalloc byte[12];

        using Stream stream = file.OpenReadStream();
        int read = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);

        if (read < header.Length)
        {
            return false;
        }

        return IsJpeg(header) || IsPng(header) || IsGif(header) || IsWebp(header);
    }

    private static bool IsJpeg(ReadOnlySpan<byte> header)
        => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static bool IsPng(ReadOnlySpan<byte> header)
        => header[..8].SequenceEqual(PngSignature);

    private static bool IsGif(ReadOnlySpan<byte> header)
        => header[..4].SequenceEqual("GIF8"u8);

    private static bool IsWebp(ReadOnlySpan<byte> header)
        => header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8);
}
