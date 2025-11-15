using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ValueCommands;

public class CreateValueCommandHandler(IValueRepository valueRepository, IMapper mapper) :
    ICommandHandler<CreateValueCommand, ValueResponseModel>
{
    public async Task<ValueResponseModel> ExecuteCommandAsync(CreateValueCommand command,
        CancellationToken cancellationToken)
    {
        var value = mapper.Map<Value>(command.Model);

        await valueRepository.AddValueAsync(value, cancellationToken);
        await valueRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ValueResponseModel>(value);
    }
}

public record CreateValueCommand(CreateValueModel Model);