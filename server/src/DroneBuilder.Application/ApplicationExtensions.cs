using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator.Mediator>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddValidatorsFromAssembly(typeof(ApplicationExtensions).Assembly);

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
