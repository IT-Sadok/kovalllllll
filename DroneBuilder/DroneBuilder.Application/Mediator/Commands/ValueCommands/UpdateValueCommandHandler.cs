using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ValueCommands;

public class UpdateValueCommandHandler(IValueRepository valueRepository, IMapper mapper) :
    ICommandHandler<UpdateValueCommand, ValueResponseModel>
{
    public async Task<ValueResponseModel> ExecuteCommandAsync(UpdateValueCommand command,
        CancellationToken cancellationToken)
    {
        var value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);

        if (value is null)
        {
            throw new Exception($"Value with id {command.ValueId} not found.");
        }

        mapper.Map(command.Model, value);

        await valueRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ValueResponseModel>(value);
    }
}

public record UpdateValueCommand(Guid ValueId, UpdateValueModel Model);