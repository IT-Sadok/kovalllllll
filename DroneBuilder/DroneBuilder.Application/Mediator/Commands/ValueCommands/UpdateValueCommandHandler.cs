using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ValueCommands;

public class UpdateValueCommandHandler(IValueRepository valueRepository, IMapper mapper) :
    ICommandHandler<UpdateValueCommand, ValueModel>
{
    public async Task<ValueModel> ExecuteCommandAsync(UpdateValueCommand command,
        CancellationToken cancellationToken)
    {
        var value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);

        if (value is null)
        {
            throw new NotFoundException($"Value with id {command.ValueId} not found.");
        }

        if (command.Model.Text is not null)
            value.Text = command.Model.Text;

        await valueRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ValueModel>(value);
    }
}

public record UpdateValueCommand(Guid ValueId, UpdateValueModel Model);