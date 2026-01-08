using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.NotificationModels;
using DroneBuilder.Application.Models.UserModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Mediator.Commands.UserCommands;

public class SignUpCommandHandler(UserManager<User> userManager, IUserRepository userRepository,INotificationService notificationService)
    : ICommandHandler<SignUpUserCommand>
{
    public async Task ExecuteCommandAsync(SignUpUserCommand command, CancellationToken cancellationToken)
    {
        var createResult = await userManager.CreateAsync(command.Model.ToEntity(), command.Model.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        await notificationService.SendNotificationAsync(
            new RegistrationNotificationModel(command.Model.Email, command.Model.Email));
        
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}

public record SignUpUserCommand(SignUpModel Model);