using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ImageEvents;
using FluentResults;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ImageCommandTests;

public class UploadImageCommandHandlerTests
{
    private readonly IImageRepository _imageRepository;
    private readonly IAzureStorageService _azureStorageService;
    private readonly IOutboxEventService _outboxService;
    private readonly UploadImageCommandHandler _handler;

    private const string ImageQueueName = "image-queue";
    private static readonly Guid ProductId = Guid.NewGuid();
    private const string UploadedImageUrl = "https://storage.azure.com/container/uploaded-image.jpg";
    private const string FileName = "test-image.jpg";

    public UploadImageCommandHandlerTests()
    {
        // Arrange
        _imageRepository = Substitute.For<IImageRepository>();
        _azureStorageService = Substitute.For<IAzureStorageService>();
        _outboxService = Substitute.For<IOutboxEventService>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            ImageQueue = new QueueConfiguration { Name = ImageQueueName }
        };

        _handler = new UploadImageCommandHandler(
            _imageRepository,
            _azureStorageService,
            _outboxService,
            queuesConfig);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUploadSucceeds_ShouldSaveImageAndReturnModel()
    {
        // Arrange
        IFormFile mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        var expectedImageModel = new ImageModel
        {
            Url = UploadedImageUrl,
            FileName = FileName
        };

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        // Act
        Result<ImageModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(UploadedImageUrl, result.Value.Url);
        Assert.Equal(FileName, result.Value.FileName);

        await _azureStorageService.Received(1).UploadFileAsync(
            Arg.Is<IFormFile>(f => f.FileName == FileName),
            Arg.Any<CancellationToken>());

        await _imageRepository.Received(1).AddImageAsync(
            Arg.Is<Image>(img =>
                img.ProductId == ProductId &&
                img.Url == UploadedImageUrl &&
                img.FileName == FileName),
            Arg.Any<CancellationToken>());

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<ImageUploadedEvent>(e => e.ProductId == ProductId),
            Arg.Is<string>(q => q == ImageQueueName),
            Arg.Any<CancellationToken>());

        await _imageRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUploadFails_ShouldThrowValidationException()
    {
        // Arrange
        IFormFile mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((false, string.Empty));

        // Act & Assert
        Result<ImageModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ValidationError>());
        Assert.Equal("Failed to upload image to storage.", result.Errors[0].Message);

        await _imageRepository.DidNotReceive().AddImageAsync(
            Arg.Is<Image>(img =>
                img.ProductId == ProductId &&
                img.Url == UploadedImageUrl &&
                img.FileName == FileName),
            Arg.Any<CancellationToken>());

        await _outboxService.DidNotReceive().StoreEventAsync(
            Arg.Is<ImageUploadedEvent>(e => e.ProductId == ProductId),
            Arg.Is<string>(q => q == ImageQueueName),
            Arg.Any<CancellationToken>());

        await _imageRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldCreateImageWithCorrectProperties()
    {
        // Arrange
        IFormFile mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        Image capturedImage = null;
        await _imageRepository.AddImageAsync(
            Arg.Do<Image>(img => capturedImage = img),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedImage);
        Assert.Equal(ProductId, capturedImage.ProductId);
        Assert.Equal(UploadedImageUrl, capturedImage.Url);
        Assert.Equal(FileName, capturedImage.FileName);
        Assert.True((DateTime.UtcNow - capturedImage.UploadedAt).TotalSeconds < 5);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldGenerateCorrectEvent()
    {
        // Arrange
        IFormFile mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        Guid capturedImageId = Guid.Empty;
        Guid capturedProductId = Guid.Empty;

        await _outboxService.StoreEventAsync(
            Arg.Do<ImageUploadedEvent>(e =>
            {
                capturedImageId = e.ImageId;
                capturedProductId = e.ProductId;
            }),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, capturedImageId);
        Assert.Equal(ProductId, capturedProductId);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldPassCorrectQueueName()
    {
        // Arrange
        IFormFile mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        string capturedQueueName = null;
        await _outboxService.StoreEventAsync(
            Arg.Any<ImageUploadedEvent>(),
            Arg.Do<string>(q => capturedQueueName = q),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(ImageQueueName, capturedQueueName);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldMapImageToModel()
    {
        // Arrange
        IFormFile mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        var expectedImageModel = new ImageModel
        {
            Url = UploadedImageUrl,
            FileName = FileName
        };

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));



        // Act
        Result<ImageModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.Equal(UploadedImageUrl, result.Value.Url);
        Assert.Equal(FileName, result.Value.FileName);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenDifferentProducts_ShouldUseCorrectProductId()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        IFormFile mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command1 = new UploadImageCommand(mockFile, productId1);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        // Act
        await _handler.ExecuteCommandAsync(command1, CancellationToken.None);

        // Assert
        await _imageRepository.Received(1).AddImageAsync(
            Arg.Is<Image>(img => img.ProductId == productId1),
            Arg.Any<CancellationToken>());

        await _imageRepository.DidNotReceive().AddImageAsync(
            Arg.Is<Image>(img => img.ProductId == productId2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldSetUploadedAtToUtcNow()
    {
        // Arrange
        IFormFile mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);
        DateTime beforeExecution = DateTime.UtcNow;

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        Image capturedImage = null;
        await _imageRepository.AddImageAsync(
            Arg.Do<Image>(img => capturedImage = img),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        DateTime afterExecution = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedImage);
        Assert.InRange(capturedImage.UploadedAt, beforeExecution, afterExecution);
    }
}

