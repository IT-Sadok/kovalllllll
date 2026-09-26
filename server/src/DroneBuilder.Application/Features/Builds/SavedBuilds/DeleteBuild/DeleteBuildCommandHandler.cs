using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds.DeleteBuild;

public class DeleteBuildCommandHandler(IBuildRepository buildRepository, IUserContext userContext)
    : ICommandHandler<DeleteBuildCommand>
{
    public async Task<Result> ExecuteCommandAsync(DeleteBuildCommand command, CancellationToken cancellationToken)
    {
        Build? build = await buildRepository.GetUserBuildAsync(command.BuildId, userContext.UserId, cancellationToken);
        if (build is null)
        {
            return Result.Fail(new NotFoundError($"Build with id {command.BuildId} not found."));
        }

        buildRepository.Remove(build);
        await buildRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record DeleteBuildCommand(Guid BuildId);
