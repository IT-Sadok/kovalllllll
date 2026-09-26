using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Warehouses.RestockEmptyItems;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.WarehouseEvents;
using FluentResults;
using FluentValidation.Results;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Warehouses;

public class RestockEmptyItemsCommandHandlerTests
{
    private const string WarehouseQueueName = "warehouse-queue";

    private readonly IWarehouseRepository _warehouseRepository = Substitute.For<IWarehouseRepository>();
    private readonly IOutboxEventService _outboxService = Substitute.For<IOutboxEventService>();

    [Fact]
    public async Task ExecuteCommandAsync_ShouldSetEveryEmptyItemAndRecordAnEventForEach()
    {
        // Arrange
        var first = new WarehouseItem { ProductId = Guid.NewGuid(), Quantity = 0 };
        var second = new WarehouseItem { ProductId = Guid.NewGuid(), Quantity = 0 };
        _warehouseRepository.GetEmptyWarehouseItemsAsync(Arg.Any<CancellationToken>()).Returns([first, second]);

        var handler = new RestockEmptyItemsCommandHandler(_warehouseRepository, _outboxService,
            new MessageQueuesConfiguration { WarehouseQueue = new QueueConfiguration { Name = WarehouseQueueName } });

        // Act
        Result<RestockResultModel> result =
            await handler.ExecuteCommandAsync(new RestockEmptyItemsCommand(25), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value.RestockedItems);
        Assert.All(new[] { first, second }, item => Assert.Equal(25, item.Quantity));
        await _outboxService.Received(2).StoreEventAsync(Arg.Is<AddedQuantityToWarehouseItemEvent>(e => e != null),
            WarehouseQueueName, Arg.Any<CancellationToken>());
        await _warehouseRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    public void Validator_WhenQuantityIsOutOfRange_ShouldFail(int quantity)
    {
        // Act
        ValidationResult result = new RestockEmptyItemsCommandValidator().Validate(new RestockEmptyItemsCommand(quantity));

        // Assert
        Assert.False(result.IsValid);
    }
}
