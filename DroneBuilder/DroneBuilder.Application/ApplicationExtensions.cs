using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator.Mediator>();
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton(MapsterConfig.Configure());

        services.Scan(scan => scan
            .FromAssemblies(typeof(ApplicationExtensions).Assembly)
            .AddClasses(classes => classes.AssignableToAny(
                typeof(ICommandHandler<>),
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}