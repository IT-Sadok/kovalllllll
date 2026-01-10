using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.NotificationModels;
using DroneBuilder.Application.Models.UserModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Mediator.Commands.UserCommands;

public class SignUpCommandHandler(
    UserManager<User> userManager,
    IUserRepository userRepository,
    IOutboxEventService outboxService)
    : ICommandHandler<SignUpUserCommand>
{
    public async Task ExecuteCommandAsync(SignUpUserCommand command, CancellationToken cancellationToken)
    {
        var user = command.Model.ToEntity();
        var createResult = await userManager.CreateAsync(user, command.Model.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        var @event = new UserSignedUpEvent(user.Id, user.Email);
        await outboxService.PublishEventAsync(@event, "user-signed-up-queue", cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}

public record SignUpUserCommand(SignUpModel Model);