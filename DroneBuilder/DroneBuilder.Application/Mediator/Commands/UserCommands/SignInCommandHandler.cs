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
    IJwtService jwtService,
    IUserRepository userRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<SignInCommand, AuthUserModel>
{
    public async Task<Result<AuthUserModel>> ExecuteCommandAsync(SignInCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(command.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, command.Password))
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

        var @event = new UserSignedInEvent(user.Id, user.Email);
        await outboxService.StoreEventAsync(@event, queuesConfig.UserQueue.Name, cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(authUserModel);
    }
}

public record SignInCommand(string Email, string Password);
