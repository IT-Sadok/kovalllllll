using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Imports.RaceDayQuads;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Imports.StartRaceDayQuadsImport;

public class StartRaceDayQuadsImportCommandHandler(IImportRunRepository importRunRepository, IImportQueue importQueue)
    : ICommandHandler<StartRaceDayQuadsImportCommand, ImportRunModel>
{
    public async Task<Result<ImportRunModel>> ExecuteCommandAsync(StartRaceDayQuadsImportCommand command,
        CancellationToken cancellationToken)
    {
        if (await importRunRepository.HasActiveRunAsync(RaceDayQuadsCatalog.Source, cancellationToken))
        {
            return Result.Fail<ImportRunModel>(new ConflictError("A RaceDayQuads import is already queued or running."));
        }

        var run = new ImportRun
        {
            Source = RaceDayQuadsCatalog.Source,
            Status = ImportRunStatus.Queued,
            CreatedAt = DateTime.UtcNow
        };

        await importRunRepository.AddAsync(run, cancellationToken);
        await importRunRepository.SaveChangesAsync(cancellationToken);
        await importQueue.EnqueueAsync(run.Id, cancellationToken);

        return Result.Ok(run.ToModel());
    }
}

public record StartRaceDayQuadsImportCommand;
