using System.Text;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.UserModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Constants;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.UserEvents;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace DroneBuilder.Application.Mediator.Commands.UserCommands;

public class SignUpCommandHandler(
    UserManager<User> userManager,
    IUserRepository userRepository,
    IOutboxEventService outboxService,
    IEmailSender emailSender,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<SignUpUserCommand>
{
    public async Task<Result> ExecuteCommandAsync(SignUpUserCommand command, CancellationToken cancellationToken)
    {
        User user = command.Model.ToEntity();
        IdentityResult createResult = await userManager.CreateAsync(user, command.Model.Password);
        if (!createResult.Succeeded)
        {
            string errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Fail(new BadRequestError($"User creation failed: {errors}"));
        }

        IdentityResult roleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            string errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            await userManager.DeleteAsync(user);
            return Result.Fail(new BadRequestError($"Could not assign the default role: {errors}"));
        }

        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        Result emailResult =
            await emailSender.SendEmailConfirmationAsync(user.Email!, user.Id, encodedToken, cancellationToken);

        if (emailResult.IsFailed)
        {
            await userManager.DeleteAsync(user);
            return emailResult;
        }

        if (user.Email != null)
        {
            var @event = new UserSignedUpEvent(user.Id, user.Email);
            await outboxService.StoreEventAsync(@event, queuesConfig.UserQueue.Name, cancellationToken);
        }

        await userRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record SignUpUserCommand(SignUpModel Model);
