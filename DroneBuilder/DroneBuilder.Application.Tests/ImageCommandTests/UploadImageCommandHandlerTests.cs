using System.ComponentModel.DataAnnotations;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ImageEvents;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ImageCommandTests;

public class UploadImageCommandHandlerTests
{
    private readonly IImageRepository _imageRepository;
    private readonly IAzureStorageService _azureStorageService;
    private readonly IOutboxEventService _outboxService;
    private readonly IMapper _mapper;
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
        _mapper = Substitute.For<IMapper>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            ImageQueue = new QueueConfiguration { Name = ImageQueueName }
        };

        _handler = new UploadImageCommandHandler(
            _imageRepository,
            _azureStorageService,
            _outboxService,
            queuesConfig,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUploadSucceeds_ShouldSaveImageAndReturnModel()
    {
        // Arrange
        var mockFile = Substitute.For<IFormFile>();
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

        _mapper.Map<ImageModel>(Arg.Is<Image>(img =>
                img.ProductId == ProductId &&
                img.Url == UploadedImageUrl &&
                img.FileName == FileName))
            .Returns(expectedImageModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UploadedImageUrl, result.Url);
        Assert.Equal(FileName, result.FileName);

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

        _mapper.Received(1).Map<ImageModel>(Arg.Is<Image>(img =>
            img.ProductId == ProductId &&
            img.Url == UploadedImageUrl &&
            img.FileName == FileName));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUploadFails_ShouldThrowValidationException()
    {
        // Arrange
        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((false, string.Empty));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Failed to upload image to storage.", exception.Message);

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

        _mapper.DidNotReceive().Map<ImageModel>(Arg.Is<Image>(img =>
            img.ProductId == ProductId &&
            img.Url == UploadedImageUrl &&
            img.FileName == FileName));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldCreateImageWithCorrectProperties()
    {
        // Arrange
        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        _mapper.Map<ImageModel>(Arg.Is<Image>(img =>
                img.ProductId == ProductId &&
                img.Url == UploadedImageUrl &&
                img.FileName == FileName))
            .Returns(new ImageModel());

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
        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        _mapper.Map<ImageModel>(Arg.Is<Image>(img =>
                img.ProductId == ProductId &&
                img.Url == UploadedImageUrl &&
                img.FileName == FileName))
            .Returns(new ImageModel());

        var capturedImageId = Guid.Empty;
        var capturedProductId = Guid.Empty;

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
        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        _mapper.Map<ImageModel>(Arg.Is<Image>(img =>
                img.ProductId == ProductId &&
                img.Url == UploadedImageUrl &&
                img.FileName == FileName))
            .Returns(new ImageModel());

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
        var mockFile = Substitute.For<IFormFile>();
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

        Image mappedImage = null;
        _mapper.Map<ImageModel>(Arg.Do<Image>(img => mappedImage = img))
            .Returns(expectedImageModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(mappedImage);
        Assert.Equal(UploadedImageUrl, mappedImage.Url);
        Assert.Equal(FileName, mappedImage.FileName);
        Assert.Equal(ProductId, mappedImage.ProductId);
        Assert.Same(expectedImageModel, result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenDifferentProducts_ShouldUseCorrectProductId()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command1 = new UploadImageCommand(mockFile, productId1);

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        _mapper.Map<ImageModel>(Arg.Is<Image>(img => img.ProductId == productId1))
            .Returns(new ImageModel());

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
        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns(FileName);

        var command = new UploadImageCommand(mockFile, ProductId);
        var beforeExecution = DateTime.UtcNow;

        _azureStorageService.UploadFileAsync(
                Arg.Is<IFormFile>(f => f.FileName == FileName),
                Arg.Any<CancellationToken>())
            .Returns((true, UploadedImageUrl));

        _mapper.Map<ImageModel>(Arg.Is<Image>(img => img.UploadedAt >= beforeExecution))
            .Returns(new ImageModel());

        Image capturedImage = null;
        await _imageRepository.AddImageAsync(
            Arg.Do<Image>(img => capturedImage = img),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        var afterExecution = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedImage);
        Assert.InRange(capturedImage.UploadedAt, beforeExecution, afterExecution);
    }
}