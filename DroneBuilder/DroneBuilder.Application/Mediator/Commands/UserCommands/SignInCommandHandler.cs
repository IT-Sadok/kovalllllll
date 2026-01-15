using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.UserModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events;
using DroneBuilder.Domain.Events.UserEvents;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Mediator.Commands.UserCommands;

public class SignInCommandHandler(
    UserManager<User> userManager,
    IJwtService jwtService,
    IUserRepository userRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IMapper mapper)
    : ICommandHandler<SignInCommand, AuthUserModel>
{
    public async Task<AuthUserModel> ExecuteCommandAsync(SignInCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            throw new InvalidEmailOrPasswordException("Invalid email or password.");
        }

        var token = await jwtService.GenerateJwtTokenAsync(user);

        var authUserModel = mapper.Map<AuthUserModel>(token);

        var @event = new UserSignedInEvent(user.Id, user.Email);
        await outboxService.PublishEventAsync(@event, queuesConfig.UserQueue, cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);

        return authUserModel;
    }
}

public record SignInCommand(string Email, string Password);