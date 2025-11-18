using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.ImageQueries;
using DroneBuilder.Application.Mediator.Queries.ProductQueries;
using DroneBuilder.Application.Mediator.Queries.PropertyQueries;
using DroneBuilder.Application.Mediator.Queries.ValueQueries;
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
        services
            .AddScoped<ICommandHandler<CreateValueCommand, ValueResponseModel>, CreateValueCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteValueCommand>, DeleteValueCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateValueCommand, ValueResponseModel>, UpdateValueCommandHandler>();
        services.AddScoped<ICommandHandler<UploadImageCommand, ImageResponseModel>, UploadImageCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteImageCommand>, DeleteImageCommandHandler>();


        services.AddScoped<IQueryHandler<GetProductsQuery, ProductsResponseModel>, GetProductsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductResponseModel>, GetProductByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>,
            GetPropertiesByProductIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertiesQuery, PropertiesResponseModel>, GetPropertiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetValuesByPropertyIdQuery, PropertyResponseModel>,
            GetValuesByPropertyIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetValuesQuery, ValuesResponseModel>, GetValuesQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertyByIdQuery, PropertyResponseModel>, GetPropertyByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetImagesQuery, ImagesResponseModel>, GetImagesQueryHandler>();
        services.AddScoped<IQueryHandler<GetImagesByProductIdQuery, ProductImagesResponseModel>,
            GetImagesByProductIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetImageByIdQuery, ImageResponseModel>, GetImageByIdQueryHandler>();


        services.AddSingleton(MapsterConfig.Configure());
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}