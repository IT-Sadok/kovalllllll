using DroneBuilder.Application.Features.Images.UploadImage;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
namespace DroneBuilder.Application.Tests.Validators.Commands;

public class UploadImageCommandValidatorTests
{
    private readonly UploadImageCommandValidator _validator;

    private static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];

    public UploadImageCommandValidatorTests()
    {
        _validator = new UploadImageCommandValidator();
    }

    private static IFormFile CreateFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static UploadImageCommand CreateCommand(string fileName, string contentType, byte[] content)
        => new(CreateFile(fileName, contentType, content), Guid.NewGuid());

    [Fact]
    public void Should_Not_Have_Error_When_File_Is_A_Real_Png()
    {
        // Arrange
        UploadImageCommand command = CreateCommand("photo.png", "image/png", PngBytes);

        // Act
        TestValidationResult<UploadImageCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Extension_Is_Not_Allowed()
    {
        // Arrange
        UploadImageCommand command = CreateCommand("payload.svg", "image/png", PngBytes);

        // Act
        TestValidationResult<UploadImageCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File.FileName);
    }

    [Fact]
    public void Should_Have_Error_When_ContentType_Is_Not_An_Image()
    {
        // Arrange
        UploadImageCommand command = CreateCommand("photo.png", "text/html", PngBytes);

        // Act
        TestValidationResult<UploadImageCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File.ContentType);
    }

    [Fact]
    public void Should_Have_Error_When_Content_Does_Not_Match_An_Image_Signature()
    {
        // Arrange
        byte[] html = "<html><script>alert(1)</script></html>"u8.ToArray();
        UploadImageCommand command = CreateCommand("photo.png", "image/png", html);

        // Act
        TestValidationResult<UploadImageCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File)
            .WithErrorMessage("File content is not a valid image.");
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Too_Large()
    {
        // Arrange
        byte[] oversized = new byte[(5 * 1024 * 1024) + 1];
        PngBytes.CopyTo(oversized, 0);

        UploadImageCommand command = CreateCommand("photo.png", "image/png", oversized);

        // Act
        TestValidationResult<UploadImageCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File.Length);
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Empty()
    {
        // Arrange
        UploadImageCommand command = CreateCommand("photo.png", "image/png", []);

        // Act
        TestValidationResult<UploadImageCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File.Length);
    }

    [Fact]
    public void Should_Have_Error_When_ProductId_Is_Empty()
    {
        // Arrange
        var command = new UploadImageCommand(CreateFile("photo.png", "image/png", PngBytes), Guid.Empty);

        // Act
        TestValidationResult<UploadImageCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }
}
