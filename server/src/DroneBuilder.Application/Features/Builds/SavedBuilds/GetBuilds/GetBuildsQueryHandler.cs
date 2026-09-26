using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds.GetBuilds;

public class GetBuildsQueryHandler(IBuildRepository buildRepository, IUserContext userContext)
    : IQueryHandler<GetBuildsQuery, ICollection<SavedBuildModel>>
{
    public async Task<Result<ICollection<SavedBuildModel>>> ExecuteAsync(GetBuildsQuery query,
        CancellationToken cancellationToken)
    {
        ICollection<Build> builds = await buildRepository.GetUserBuildsAsync(userContext.UserId, cancellationToken);

        return Result.Ok<ICollection<SavedBuildModel>>(builds.Select(b => b.ToModel()).ToList());
    }
}

public record GetBuildsQuery;
