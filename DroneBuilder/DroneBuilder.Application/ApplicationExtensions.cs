using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.ProductQueries;
using DroneBuilder.Application.Mediator.Queries.PropertyQueries;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Models.UserModels;
using Mapster;
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
        services
            .AddScoped<ICommandHandler<CreatePropertyCommand, PropertyResponseModel>, CreatePropertyCommandHandler>();
        services
            .AddScoped<ICommandHandler<UpdatePropertyCommand, PropertyResponseModel>, UpdatePropertyCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePropertyCommand>, DeletePropertyCommandHandler>();
        services
            .AddScoped<ICommandHandler<AddPropertyToProductCommand>, AddPropertyToProductCommandHandler>();

        services.AddScoped<IQueryHandler<GetProductsQuery, ProductsResponseModel>, GetProductsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductResponseModel>, GetProductByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>,
            GetPropertiesByProductIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertiesQuery, PropertiesResponseModel>, GetPropertiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetValuesByPropertyIdQuery, PropertyResponseModel>,
            GetValuesByPropertyIdQueryHandler>();


        services.AddSingleton(MapsterConfig.Configure());
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}