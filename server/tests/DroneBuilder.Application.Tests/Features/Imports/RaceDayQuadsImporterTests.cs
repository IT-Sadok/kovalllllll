using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Imports.RaceDayQuads;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;
using DroneBuilder.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Imports;

public class RaceDayQuadsImporterTests
{
    private readonly IRaceDayQuadsClient _client;
    private readonly IImportRunRepository _importRunRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxEventService _outboxService;
    private readonly RaceDayQuadsImporter _importer;

    private readonly ImportRun _run = new() { Source = "racedayquads", Status = ImportRunStatus.Queued };
    private static readonly Guid WarehouseId = Guid.NewGuid();

    private static readonly ShopifyProduct Motor = new(
        77, "RDQ Badass 2 - 2207.5 Motor", "rdq-badass-2",
        "<p>Rated Voltage (LiPo): 4-6S</p>", "RDQ", "Motor",
        ["Manufacturer_RDQ", "Motor Bolt Pattern_16x16mm", "Stator Size_2207.5"],
        [new ShopifyVariant(701, "1400Kv", "14.29"), new ShopifyVariant(702, "1900Kv", "14.29")],
        [new ShopifyImage("https://cdn.shopify.com/s/files/motor.jpg?v=1")]);

    public RaceDayQuadsImporterTests()
    {
        // Arrange
        _client = Substitute.For<IRaceDayQuadsClient>();
        _importRunRepository = Substitute.For<IImportRunRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _outboxService = Substitute.For<IOutboxEventService>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(Substitute.For<ITransaction>());
        _importRunRepository.GetByIdAsync(_run.Id, Arg.Any<CancellationToken>()).Returns(_run);
        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>()).Returns(new Warehouse { Id = WarehouseId });
        _client.GetCollectionProductsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _client.GetCollectionProductsAsync("22xx-brushless-motors", 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([Motor]);
        _productRepository.GetByExternalIdsAsync(Arg.Any<string>(), Arg.Any<ICollection<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var queues = new MessageQueuesConfiguration { ProductQueue = new QueueConfiguration { Name = "product-queue" } };

        _importer = new RaceDayQuadsImporter(_client, _importRunRepository, _productRepository, _warehouseRepository,
            _unitOfWork, _outboxService, queues, new RaceDayQuadsImportOptions(),
            NullLogger<RaceDayQuadsImporter>.Instance);
    }

    [Fact]
    public async Task RunAsync_WhenProductIsNew_ShouldCreateVariantsInOneGroupWithStock()
    {
        // Act
        await _importer.RunAsync(_run.Id, CancellationToken.None);

        // Assert
        await _productRepository.Received(1).AddGroupAsync(
            Arg.Is<ProductGroup>(g => g.ExternalId == "77" && g.Name == "RDQ Badass 2 - 2207.5 Motor"),
            Arg.Any<CancellationToken>());
        await _productRepository.Received(1).AddProductAsync(
            Arg.Is<Product>(p => p.ExternalId == "702" && p.Category == ProductCategory.Motor
                                 && p.Spec is MotorSpec && ((MotorSpec)p.Spec).Kv == 1900 && !p.NeedsReview
                                 && p.Group != null && p.Images.Single().IsPrimary),
            Arg.Any<CancellationToken>());
        await _warehouseRepository.Received(2).AddWarehouseItemAsync(
            Arg.Is<WarehouseItem>(w => w.WarehouseId == WarehouseId && w.Quantity == 0), Arg.Any<CancellationToken>());
        await _outboxService.Received(2).StoreEventAsync(Arg.Any<DomainEvent>(), "product-queue", Arg.Any<CancellationToken>());

        Assert.Equal(ImportRunStatus.Succeeded, _run.Status);
        Assert.Equal(2, _run.Added);
        Assert.Equal(0, _run.Failed);
        Assert.NotNull(_run.FinishedAt);
    }

    [Fact]
    public async Task RunAsync_WhenProductWasDelistedByAdmin_ShouldSkipIt()
    {
        // Arrange
        _productRepository.GetByExternalIdsAsync(Arg.Any<string>(), Arg.Any<ICollection<string>>(), Arg.Any<CancellationToken>())
            .Returns([
                new Product { ExternalSource = "racedayquads", ExternalId = "701", IsDeleted = true, Name = "old" },
                new Product { ExternalSource = "racedayquads", ExternalId = "702", Category = ProductCategory.Motor }
            ]);

        // Act
        await _importer.RunAsync(_run.Id, CancellationToken.None);

        // Assert
        Assert.Equal(1, _run.Skipped);
        Assert.Equal(1, _run.Updated);
        Assert.Equal(0, _run.Added);
        await _productRepository.DidNotReceive().AddProductAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenRunIsNotQueued_ShouldDoNothing()
    {
        // Arrange
        _run.Status = ImportRunStatus.Succeeded;

        // Act
        await _importer.RunAsync(_run.Id, CancellationToken.None);

        // Assert
        await _client.DidNotReceive().GetCollectionProductsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenSourceIsUnreachable_ShouldMarkRunFailed()
    {
        // Arrange
        _client.GetCollectionProductsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<ShopifyProduct>>(_ => throw new HttpRequestException("503 Service Unavailable"));

        // Act
        await _importer.RunAsync(_run.Id, CancellationToken.None);

        // Assert
        Assert.Equal(ImportRunStatus.Failed, _run.Status);
        Assert.Equal("503 Service Unavailable", _run.Error);
        Assert.NotNull(_run.FinishedAt);
    }
}
