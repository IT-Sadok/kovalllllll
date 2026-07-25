using DroneBuilder.Application.Common;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Values.CreateValue;

public class CreateValueCommandHandler(
    IValueRepository valueRepository,
    IPropertyRepository propertyRepository)
    : ICommandHandler<CreateValueCommand, ValueModel>
{
    public async Task<Result<ValueModel>> ExecuteCommandAsync(
        CreateValueCommand command,
        CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(
            command.Model.PropertyId,
            cancellationToken);
        if (property is null)
        {
            return Result.Fail<ValueModel>(new NotFoundError(
                $"Property with id {command.Model.PropertyId} not found."));
        }

        if (property.DataType != SpecificationDataType.Option)
        {
            return Result.Fail<ValueModel>(new ValidationError(
                $"Property '{property.Code}' does not accept predefined option values."));
        }

        string code = EntityCode.FromName(command.Model.Code ?? command.Model.Text);
        if (await valueRepository.IsCodeInUseAsync(code, cancellationToken: cancellationToken))
        {
            return Result.Fail<ValueModel>(new ConflictError($"Value code '{code}' already exists."));
        }

        if (command.Model.NumericValue.HasValue && command.Model.BooleanValue.HasValue)
        {
            return Result.Fail<ValueModel>(new ValidationError(
                "An option cannot have both numeric and boolean canonical values."));
        }

        Value value = command.Model.ToEntity();
        value.Code = code;
        property.Values.Add(value);
        value.Properties.Add(property);
        await valueRepository.AddValueAsync(value, cancellationToken);
        await valueRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(value.ToModel());
    }
}

public sealed record CreateValueCommand(CreateValueModel Model);
