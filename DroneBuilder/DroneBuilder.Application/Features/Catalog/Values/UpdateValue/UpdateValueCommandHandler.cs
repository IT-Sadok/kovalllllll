using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Values.UpdateValue;

public class UpdateValueCommandHandler(IValueRepository valueRepository) :
    ICommandHandler<UpdateValueCommand, ValueModel>
{
    public async Task<Result<ValueModel>> ExecuteCommandAsync(UpdateValueCommand command,
        CancellationToken cancellationToken)
    {
        Value? value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);

        if (value is null)
        {
            return Result.Fail<ValueModel>(new NotFoundError($"Value with id {command.ValueId} not found."));
        }

        if (command.Model.Text is not null)
        {
            value.Text = command.Model.Text;
        }

        await valueRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(value.ToModel());
    }
}

public record UpdateValueCommand(Guid ValueId, UpdateValueModel Model);
