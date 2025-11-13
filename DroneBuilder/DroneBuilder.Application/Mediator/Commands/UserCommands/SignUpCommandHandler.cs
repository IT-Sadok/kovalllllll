using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Mediator.Commands;

public class SignUpCommandHandler(UserManager<User> userManager)
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
    }
}

public record SignUpUserCommand(SignUpModel Model);