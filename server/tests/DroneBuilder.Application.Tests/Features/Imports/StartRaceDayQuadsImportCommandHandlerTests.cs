using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Imports;
using DroneBuilder.Application.Features.Imports.StartRaceDayQuadsImport;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Imports;

public class StartRaceDayQuadsImportCommandHandlerTests
{
    private readonly IImportRunRepository _importRunRepository;
    private readonly IImportQueue _importQueue;
    private readonly StartRaceDayQuadsImportCommandHandler _handler;

    public StartRaceDayQuadsImportCommandHandlerTests()
    {
        // Arrange
        _importRunRepository = Substitute.For<IImportRunRepository>();
        _importQueue = Substitute.For<IImportQueue>();

        _handler = new StartRaceDayQuadsImportCommandHandler(_importRunRepository, _importQueue);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenNoActiveRun_ShouldQueueNewRun()
    {
        // Arrange
        _importRunRepository.HasActiveRunAsync("racedayquads", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<ImportRunModel> result =
            await _handler.ExecuteCommandAsync(new StartRaceDayQuadsImportCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ImportRunStatus.Queued, result.Value.Status);
        await _importRunRepository.Received(1).AddAsync(
            Arg.Is<ImportRun>(r => r.Source == "racedayquads" && r.Status == ImportRunStatus.Queued),
            Arg.Any<CancellationToken>());
        await _importRunRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _importQueue.Received(1).EnqueueAsync(result.Value.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenRunIsActive_ShouldReturnConflict()
    {
        // Arrange
        _importRunRepository.HasActiveRunAsync("racedayquads", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result<ImportRunModel> result =
            await _handler.ExecuteCommandAsync(new StartRaceDayQuadsImportCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.HasError<ConflictError>());
        await _importQueue.DidNotReceive().EnqueueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
