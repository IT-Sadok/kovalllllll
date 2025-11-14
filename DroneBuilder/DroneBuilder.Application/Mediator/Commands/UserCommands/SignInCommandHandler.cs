using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Application.Mediator.Commands.UserCommands;

public class SignInCommandHandler(UserManager<User> userManager, IJwtService jwtService, IMapper mapper)
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

        return mapper.Map<AuthUserModel>(token);
    }
}

public record SignInCommand(string Email, string Password);