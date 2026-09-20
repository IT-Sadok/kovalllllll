using System.Text;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace DroneBuilder.Application.Mediator.Commands.UserCommands;

public class ConfirmEmailCommandHandler(UserManager<User> userManager) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task<Result> ExecuteCommandAsync(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return Result.Fail(new NotFoundError($"User with id {command.UserId} not found."));
        }

        // Following the same link twice should not look like a failure.
        if (user.EmailConfirmed)
        {
            return Result.Ok();
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Token));
        }
        catch (FormatException)
        {
            return Result.Fail(new BadRequestError("Confirmation token is malformed."));
        }

        IdentityResult confirmResult = await userManager.ConfirmEmailAsync(user, token);
        if (!confirmResult.Succeeded)
        {
            return Result.Fail(new BadRequestError("Confirmation token is invalid or has expired."));
        }

        return Result.Ok();
    }
}

public record ConfirmEmailCommand(Guid UserId, string Token);
