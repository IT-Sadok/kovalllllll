using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.UserModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.UserEvents;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Mediator.Commands.UserCommands;

public class SignUpCommandHandler(
    UserManager<User> userManager,
    IUserRepository userRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<SignUpUserCommand>
{
    public async Task ExecuteCommandAsync(SignUpUserCommand command, CancellationToken cancellationToken)
    {
        User user = command.Model.ToEntity();
        IdentityResult createResult = await userManager.CreateAsync(user, command.Model.Password);
        if (!createResult.Succeeded)
        {
            string errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        if (user.Email != null)
        {
            var @event = new UserSignedUpEvent(user.Id, user.Email);
            await outboxService.StoreEventAsync(@event, queuesConfig.UserQueue.Name, cancellationToken);
        }

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}

public record SignUpUserCommand(SignUpModel Model);
