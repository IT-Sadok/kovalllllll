using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.UserModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.UserEvents;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Mediator.Commands.UserCommands;

public class SignInCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IJwtService jwtService,
    IUserRepository userRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<SignInCommand, AuthUserModel>
{
    public async Task<Result<AuthUserModel>> ExecuteCommandAsync(SignInCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            return Result.Fail<AuthUserModel>(new UnauthorizedError("Invalid email or password."));
        }

        // SignInManager is what applies the configured lockout: it counts failures and blocks
        // unconfirmed accounts. UserManager.CheckPasswordAsync does neither.
        SignInResult signInResult =
            await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return Result.Fail<AuthUserModel>(
                new ForbiddenError("Account is temporarily locked after too many failed attempts."));
        }

        if (signInResult.IsNotAllowed)
        {
            return Result.Fail<AuthUserModel>(
                new ForbiddenError("Please confirm your email address before signing in."));
        }

        if (!signInResult.Succeeded)
        {
            return Result.Fail<AuthUserModel>(new UnauthorizedError("Invalid email or password."));
        }

        Result<string> tokenResult = await jwtService.GenerateJwtTokenAsync(user.Id.ToString());
        if (tokenResult.IsFailed)
        {
            return tokenResult.ToResult<AuthUserModel>();
        }

        AuthUserModel authUserModel = new()
        { AccessToken = tokenResult.Value };

        var @event = new UserSignedInEvent(user.Id, user.Email ?? string.Empty);
        await outboxService.StoreEventAsync(@event, queuesConfig.UserQueue.Name, cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(authUserModel);
    }
}

public record SignInCommand(string Email, string Password);
