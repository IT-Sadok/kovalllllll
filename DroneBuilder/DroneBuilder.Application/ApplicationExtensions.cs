using DroneBuilder.Application.Mediator.Commands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Services;
using DroneBuilder.Application.Services.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator.Mediator>();

        services.AddScoped<ICommandHandler<SignUpUserCommand>,
            SignUpCommandHandler>();

        services.AddScoped<ICommandHandler<SignInCommand, AuthUserModel>, SignInCommandHandler>();


        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}