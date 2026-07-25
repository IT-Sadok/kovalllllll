using DroneBuilder.Application.Common;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Properties.CreateProperty;

public class CreatePropertyCommandHandler(IPropertyRepository propertyRepository)
    : ICommandHandler<CreatePropertyCommand, PropertyModel>
{
    public async Task<Result<PropertyModel>> ExecuteCommandAsync(
        CreatePropertyCommand command,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(command.Model.DataType, ignoreCase: true, out SpecificationDataType dataType))
        {
            return Result.Fail<PropertyModel>(new ValidationError(
                $"Unsupported property data type '{command.Model.DataType}'."));
        }

        string code = EntityCode.FromName(command.Model.Code ?? command.Model.Name);
        if (await propertyRepository.IsCodeInUseAsync(code, cancellationToken: cancellationToken))
        {
            return Result.Fail<PropertyModel>(new ConflictError($"Property code '{code}' already exists."));
        }

        if (command.Model.UnitDefinitionId.HasValue &&
            await propertyRepository.GetUnitDefinitionAsync(command.Model.UnitDefinitionId.Value, cancellationToken) is null)
        {
            return Result.Fail<PropertyModel>(new NotFoundError(
                $"Unit with id {command.Model.UnitDefinitionId.Value} not found."));
        }

        if (dataType != SpecificationDataType.Option && command.Model.Values.Count > 0)
        {
            return Result.Fail<PropertyModel>(new ValidationError(
                "Only Option properties can contain predefined values."));
        }

        Property property = command.Model.ToEntity();
        property.Code = code;
        property.DataType = dataType;

        try
        {
            property.ValidateDefinition();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<PropertyModel>(new ValidationError(exception.Message));
        }

        await propertyRepository.AddPropertyAsync(property, cancellationToken);
        await propertyRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(property.ToModel());
    }
}

public sealed record CreatePropertyCommand(CreatePropertyModel Model);
