using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Imports.GetImportRunById;

public class GetImportRunByIdQueryHandler(IImportRunRepository importRunRepository)
    : IQueryHandler<GetImportRunByIdQuery, ImportRunModel>
{
    public async Task<Result<ImportRunModel>> ExecuteAsync(GetImportRunByIdQuery query,
        CancellationToken cancellationToken)
    {
        ImportRun? run = await importRunRepository.GetByIdAsync(query.ImportRunId, cancellationToken);
        return run is null
            ? Result.Fail<ImportRunModel>(new NotFoundError($"Import run with id {query.ImportRunId} not found."))
            : Result.Ok(run.ToModel());
    }
}

public record GetImportRunByIdQuery(Guid ImportRunId);
