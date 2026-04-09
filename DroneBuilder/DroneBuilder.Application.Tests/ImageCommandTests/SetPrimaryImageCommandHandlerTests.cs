using System.ComponentModel.DataAnnotations;
using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.ImageCommandTests;

public class SetPrimaryImageCommandHandlerTests
{
    private readonly IImageRepository _imageRepository;
    private readonly SetPrimaryImageCommandHandler _handler;

    private static readonly Guid ImageId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();

    public SetPrimaryImageCommandHandlerTests()
    {
        _imageRepository = Substitute.For<IImageRepository>();
        _handler = new SetPrimaryImageCommandHandler(_imageRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenImageExists_ShouldSetItToPrimaryAndOthersToFalse()
    {
        // Arrange
        var command = new SetPrimaryImageCommand(ImageId);
        var targetImage = new Image { Id = ImageId, ProductId = ProductId, IsPrimary = false };
        var otherImage = new Image { Id = Guid.NewGuid(), ProductId = ProductId, IsPrimary = true };
        var images = new List<Image> { targetImage, otherImage };

        _imageRepository.GetImageByIdAsync(ImageId, Arg.Any<CancellationToken>()).Returns(targetImage);
        _imageRepository.GetImagesByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(images);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(targetImage.IsPrimary);
        Assert.False(otherImage.IsPrimary);
        await _imageRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenImageNotFound_ShouldThrowValidationException()
    {
        // Arrange
        var command = new SetPrimaryImageCommand(ImageId);
        _imageRepository.GetImageByIdAsync(ImageId, Arg.Any<CancellationToken>()).Returns((Image)null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }
}
