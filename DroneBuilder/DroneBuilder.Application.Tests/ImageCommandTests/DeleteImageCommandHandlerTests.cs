using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ImageCommandTests;

public class DeleteImageCommandHandlerTests
{
    private readonly IAzureStorageService _azureStorageService;
    private readonly IImageRepository _imageRepository;
    private readonly DeleteImageCommandHandler _handler;

    private static readonly Guid ImageId = Guid.NewGuid();
    private const string ImageUrl = "https://storage.azure.com/container/image123.jpg";

    public DeleteImageCommandHandlerTests()
    {
        // Arrange
        _azureStorageService = Substitute.For<IAzureStorageService>();
        _imageRepository = Substitute.For<IImageRepository>();

        _handler = new DeleteImageCommandHandler(
            _azureStorageService,
            _imageRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenImageExists_ShouldDeleteFromStorageAndRepository()
    {
        // Arrange
        var command = new DeleteImageCommand(ImageId);

        var existingImage = new Image
        {
            Id = ImageId,
            Url = ImageUrl
        };

        _imageRepository.GetImageByIdAsync(ImageId, Arg.Any<CancellationToken>())
            .Returns(existingImage);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _azureStorageService.Received(1).DeleteFileAsync(
            Arg.Is<string>(url => url == ImageUrl));

        _imageRepository.Received(1).RemoveImage(
            Arg.Is<Image>(img => img.Id == ImageId && img.Url == ImageUrl));

        await _imageRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenImageNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteImageCommand(ImageId);

        _imageRepository.GetImageByIdAsync(ImageId, Arg.Any<CancellationToken>())
            .Returns((Image)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Image with id {ImageId} not found.", exception.Message);

        await _azureStorageService.DidNotReceive().DeleteFileAsync(Arg.Is<string>(url => url == ImageUrl));
        _imageRepository.DidNotReceive().RemoveImage(Arg.Is<Image>(img => img.Id == ImageId && img.Url == ImageUrl));
        await _imageRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldCallMethodsInCorrectOrder()
    {
        // Arrange
        var command = new DeleteImageCommand(ImageId);
        var callOrder = new List<string>();

        var existingImage = new Image
        {
            Id = ImageId,
            Url = ImageUrl
        };

        _imageRepository.GetImageByIdAsync(ImageId, Arg.Any<CancellationToken>())
            .Returns(existingImage);

        _azureStorageService.DeleteFileAsync(Arg.Is<string>(url => url == ImageUrl))
            .Returns(Task.CompletedTask)
            .AndDoes(_ => callOrder.Add("DeleteFileAsync"));

        _imageRepository
            .When(x => x.RemoveImage(Arg.Is<Image>(img => img.Id == ImageId && img.Url == ImageUrl)))
            .Do(_ => callOrder.Add("RemoveImage"));

        _imageRepository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(_ => callOrder.Add("SaveChangesAsync"));

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(3, callOrder.Count);
        Assert.Equal("DeleteFileAsync", callOrder[0]);
        Assert.Equal("RemoveImage", callOrder[1]);
        Assert.Equal("SaveChangesAsync", callOrder[2]);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenImageExists_ShouldPassCorrectUrlToAzureStorage()
    {
        // Arrange
        var command = new DeleteImageCommand(ImageId);
        var customImageUrl = "https://mycustomstorage.blob.core.windows.net/images/photo.png";

        var existingImage = new Image
        {
            Id = ImageId,
            Url = customImageUrl
        };

        _imageRepository.GetImageByIdAsync(ImageId, Arg.Any<CancellationToken>())
            .Returns(existingImage);

        string capturedUrl = null;
        await _azureStorageService.DeleteFileAsync(Arg.Do<string>(url => capturedUrl = url));

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedUrl);
        Assert.Equal(customImageUrl, capturedUrl);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenImageExists_ShouldRemoveExactImageEntity()
    {
        // Arrange
        var command = new DeleteImageCommand(ImageId);

        var existingImage = new Image
        {
            Id = ImageId,
            Url = ImageUrl,
            ProductId = Guid.NewGuid()
        };

        _imageRepository.GetImageByIdAsync(ImageId, Arg.Any<CancellationToken>())
            .Returns(existingImage);

        Image capturedImage = null;
        _imageRepository.When(x => x.RemoveImage(Arg.Is<Image>(img => img.Id == ImageId && img.Url == ImageUrl)))
            .Do(callInfo => capturedImage = callInfo.Arg<Image>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedImage);
        Assert.Same(existingImage, capturedImage);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenDifferentImageIds_ShouldQueryCorrectImage()
    {
        // Arrange
        var imageId1 = Guid.NewGuid();
        var imageId2 = Guid.NewGuid();

        var command1 = new DeleteImageCommand(imageId1);

        var image1 = new Image { Id = imageId1, Url = "url1.jpg" };

        _imageRepository.GetImageByIdAsync(imageId1, Arg.Any<CancellationToken>())
            .Returns(image1);

        // Act
        await _handler.ExecuteCommandAsync(command1, CancellationToken.None);

        // Assert
        await _imageRepository.Received(1).GetImageByIdAsync(
            Arg.Is<Guid>(id => id == imageId1),
            Arg.Any<CancellationToken>());

        await _imageRepository.DidNotReceive().GetImageByIdAsync(
            Arg.Is<Guid>(id => id == imageId2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenImageWithEmptyUrl_ShouldStillCallDeleteFile()
    {
        // Arrange
        var command = new DeleteImageCommand(ImageId);

        var existingImage = new Image
        {
            Id = ImageId,
            Url = string.Empty
        };

        _imageRepository.GetImageByIdAsync(ImageId, Arg.Any<CancellationToken>())
            .Returns(existingImage);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _azureStorageService.Received(1).DeleteFileAsync(string.Empty);
        _imageRepository.Received(1).RemoveImage(Arg.Is<Image>(img => img.Id == ImageId && img.Url == string.Empty));
        await _imageRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}