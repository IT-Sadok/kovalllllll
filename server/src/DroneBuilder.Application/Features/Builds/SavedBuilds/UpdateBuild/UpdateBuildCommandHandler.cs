using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds.UpdateBuild;

public class UpdateBuildCommandHandler(
    IBuildRepository buildRepository,
    IProductRepository productRepository,
    IUserContext userContext)
    : ICommandHandler<UpdateBuildCommand, SavedBuildModel>
{
    public async Task<Result<SavedBuildModel>> ExecuteCommandAsync(UpdateBuildCommand command,
        CancellationToken cancellationToken)
    {
        Build? build = await buildRepository.GetUserBuildAsync(command.BuildId, userContext.UserId, cancellationToken);
        if (build is null)
        {
            return Result.Fail<SavedBuildModel>(new NotFoundError($"Build with id {command.BuildId} not found."));
        }

        Result productsResult =
            await BuildProducts.EnsureExistAsync(productRepository, command.Model.Items, cancellationToken);
        if (productsResult.IsFailed)
        {
            return productsResult;
        }

        Dictionary<Guid, BuildItemModel> requested = command.Model.Items.ToDictionary(i => i.ProductId);

        foreach (BuildItem removed in build.Items.Where(i => !requested.ContainsKey(i.ProductId)).ToList())
        {
            build.Items.Remove(removed);
        }

        foreach (BuildItemModel item in requested.Values)
        {
            BuildItem? existing = build.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing is null)
            {
                build.Items.Add(new BuildItem { ProductId = item.ProductId, Quantity = item.Quantity });
            }
            else
            {
                existing.Quantity = item.Quantity;
            }
        }

        build.Name = command.Model.Name.Trim();
        build.UpdatedAt = DateTime.UtcNow;

        await buildRepository.SaveChangesAsync(cancellationToken);

        Build saved = (await buildRepository.GetUserBuildAsync(build.Id, userContext.UserId, cancellationToken))!;
        return Result.Ok(saved.ToModel());
    }
}

public record UpdateBuildCommand(Guid BuildId, SaveBuildModel Model);
