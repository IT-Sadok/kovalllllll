using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Features.Imports.RaceDayQuads;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Common.Mediator.Mediator>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IRaceDayQuadsImporter, RaceDayQuadsImporter>();
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
