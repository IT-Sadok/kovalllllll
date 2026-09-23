using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Constants;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.UserEvents;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Features.Users.SignUp;

public class SignUpCommandHandler(
    UserManager<User> userManager,
    IUserRepository userRepository,
    IOutboxEventService outboxService,
    IEmailSender emailSender,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<SignUpCommand>
{
    private static readonly string[] DuplicateAccountErrorCodes =
    [
        nameof(IdentityErrorDescriber.DuplicateEmail),
        nameof(IdentityErrorDescriber.DuplicateUserName)
    ];

    public async Task<Result> ExecuteCommandAsync(SignUpCommand command, CancellationToken cancellationToken)
    {
        User user = command.Model.ToEntity();
        IdentityResult createResult = await userManager.CreateAsync(user, command.Model.Password);
        if (!createResult.Succeeded)
        {
            var otherErrors = createResult.Errors
                .Where(e => !DuplicateAccountErrorCodes.Contains(e.Code))
                .ToList();

            if (otherErrors.Count > 0)
            {
                string errors = string.Join("; ", otherErrors.Select(e => e.Description));
                return Result.Fail(new BadRequestError($"User creation failed: {errors}"));
            }

            return await HandleExistingAccountAsync(command.Model.Email, cancellationToken);
        }

        IdentityResult roleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            string errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            await userManager.DeleteAsync(user);
            return Result.Fail(new BadRequestError($"Could not assign the default role: {errors}"));
        }

        Result emailResult = await userManager.SendEmailConfirmationAsync(emailSender, user, cancellationToken);

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

    private async Task<Result> HandleExistingAccountAsync(string email, CancellationToken cancellationToken)
    {
        User? existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is { EmailConfirmed: false })
        {
            await userManager.SendEmailConfirmationAsync(emailSender, existingUser, cancellationToken);
        }

        return Result.Ok();
    }
}

public record SignUpCommand(SignUpModel Model);
