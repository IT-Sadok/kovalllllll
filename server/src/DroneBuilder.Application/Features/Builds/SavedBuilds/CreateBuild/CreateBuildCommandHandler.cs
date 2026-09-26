using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds.CreateBuild;

public class CreateBuildCommandHandler(
    IBuildRepository buildRepository,
    IProductRepository productRepository,
    IUserContext userContext)
    : ICommandHandler<CreateBuildCommand, SavedBuildModel>
{
    public async Task<Result<SavedBuildModel>> ExecuteCommandAsync(CreateBuildCommand command,
        CancellationToken cancellationToken)
    {
        Result productsResult =
            await BuildProducts.EnsureExistAsync(productRepository, command.Model.Items, cancellationToken);
        if (productsResult.IsFailed)
        {
            return productsResult;
        }

        var build = new Build
        {
            UserId = userContext.UserId,
            Name = command.Model.Name.Trim(),
            Items = command.Model.Items
                .Select(i => new BuildItem { ProductId = i.ProductId, Quantity = i.Quantity })
                .ToList()
        };

        await buildRepository.AddAsync(build, cancellationToken);
        await buildRepository.SaveChangesAsync(cancellationToken);

        Build saved = (await buildRepository.GetUserBuildAsync(build.Id, userContext.UserId, cancellationToken))!;
        return Result.Ok(saved.ToModel());
    }
}

public record CreateBuildCommand(SaveBuildModel Model);
