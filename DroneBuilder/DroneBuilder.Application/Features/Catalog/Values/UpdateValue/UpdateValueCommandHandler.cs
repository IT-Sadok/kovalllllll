using DroneBuilder.Application.Common;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Values.UpdateValue;

public class UpdateValueCommandHandler(IValueRepository valueRepository)
    : ICommandHandler<UpdateValueCommand, ValueModel>
{
    public async Task<Result<ValueModel>> ExecuteCommandAsync(
        UpdateValueCommand command,
        CancellationToken cancellationToken)
    {
        Value? value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);
        if (value is null)
        {
            return Result.Fail<ValueModel>(new NotFoundError($"Value with id {command.ValueId} not found."));
        }

        string targetCode = command.Model.Code is null
            ? value.Code
            : EntityCode.FromName(command.Model.Code);
        if (await valueRepository.IsCodeInUseAsync(targetCode, value.Id, cancellationToken))
        {
            return Result.Fail<ValueModel>(new ConflictError($"Value code '{targetCode}' already exists."));
        }

        decimal? targetNumericValue = command.Model.ClearNumericValue
            ? null
            : command.Model.NumericValue ?? value.NumericValue;
        bool? targetBooleanValue = command.Model.ClearBooleanValue
            ? null
            : command.Model.BooleanValue ?? value.BooleanValue;
        if (targetNumericValue.HasValue && targetBooleanValue.HasValue)
        {
            return Result.Fail<ValueModel>(new ValidationError(
                "An option cannot have both numeric and boolean canonical values."));
        }

        value.Code = targetCode;
        value.Text = command.Model.Text?.Trim() ?? value.Text;
        value.NumericValue = targetNumericValue;
        value.BooleanValue = targetBooleanValue;

        if (command.Model.Aliases is not null)
        {
            foreach (ValueAlias alias in value.Aliases.Where(alias => alias.ImportSourceId is null).ToList())
            {
                value.Aliases.Remove(alias);
            }

            foreach (string alias in command.Model.Aliases.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                value.Aliases.Add(new ValueAlias { ValueId = value.Id, Alias = alias.Trim() });
            }
        }

        await valueRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok(value.ToModel());
    }
}

public sealed record UpdateValueCommand(Guid ValueId, UpdateValueModel Model);
