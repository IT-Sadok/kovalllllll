using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Features.Users.ResendEmailConfirmation;

public class ResendEmailConfirmationCommandHandler(UserManager<User> userManager, IEmailSender emailSender)
    : ICommandHandler<ResendEmailConfirmationCommand>
{
    public async Task<Result> ExecuteCommandAsync(ResendEmailConfirmationCommand command,
        CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(command.Email);

        if (user is { EmailConfirmed: false })
        {
            await userManager.SendEmailConfirmationAsync(emailSender, user, cancellationToken);
        }

        return Result.Ok();
    }
}

public record ResendEmailConfirmationCommand(string Email);
