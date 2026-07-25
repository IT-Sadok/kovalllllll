using DroneBuilder.Application.Common;
using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.CreateComponentType;

public sealed class CreateComponentTypeCommandHandler(ICatalogMetadataRepository repository)
    : ICommandHandler<CreateComponentTypeCommand, ComponentTypeModel>
{
    public async Task<Result<ComponentTypeModel>> ExecuteCommandAsync(
        CreateComponentTypeCommand command,
        CancellationToken cancellationToken)
    {
        string code = EntityCode.FromName(command.Model.Code);
        if (await repository.IsComponentTypeCodeInUseAsync(code, cancellationToken: cancellationToken))
        {
            return Result.Fail<ComponentTypeModel>(new ConflictError(
                $"Component type code '{code}' already exists."));
        }

        var componentType = new ComponentType
        {
            Code = code,
            Name = command.Model.Name.Trim()
        };
        await repository.AddComponentTypeAsync(componentType, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(componentType.ToModel());
    }
}

public sealed record CreateComponentTypeCommand(CreateComponentTypeModel Model);
