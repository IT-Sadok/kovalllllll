using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.ProductQueries;
using DroneBuilder.Application.Models;
using MapsterMapper;
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
        services.AddScoped<ICommandHandler<CreateProductCommand, ProductResponseModel>, CreateProductCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProductCommand, ProductResponseModel>, UpdateProductCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProductCommand>, DeleteProductCommandHandler>();

        services.AddScoped<IQueryHandler<GetProductsQuery, ProductsResponseModel>, GetProductsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductResponseModel>, GetProductByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>,
            GetPropertiesByProductIdQueryHandler>();

        services.AddSingleton(MapsterConfig.Configure());
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}