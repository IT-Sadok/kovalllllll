using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds.GetBuildById;

public class GetBuildByIdQueryHandler(IBuildRepository buildRepository, IUserContext userContext)
    : IQueryHandler<GetBuildByIdQuery, SavedBuildModel>
{
    public async Task<Result<SavedBuildModel>> ExecuteAsync(GetBuildByIdQuery query, CancellationToken cancellationToken)
    {
        Build? build = await buildRepository.GetUserBuildAsync(query.BuildId, userContext.UserId, cancellationToken);

        return build is null
            ? Result.Fail<SavedBuildModel>(new NotFoundError($"Build with id {query.BuildId} not found."))
            : Result.Ok(build.ToModel());
    }
}

public record GetBuildByIdQuery(Guid BuildId);
