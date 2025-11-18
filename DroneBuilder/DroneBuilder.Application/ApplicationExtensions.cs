using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.Filters;
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
        services.AddScoped<ICommandHandler<CreateProductCommand, ProductModel>, CreateProductCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProductCommand, ProductModel>, UpdateProductCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProductCommand>, DeleteProductCommandHandler>();
        services
            .AddScoped<ICommandHandler<CreatePropertyCommand, PropertyModel>, CreatePropertyCommandHandler>();
        services
            .AddScoped<ICommandHandler<UpdatePropertyCommand, PropertyModel>, UpdatePropertyCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePropertyCommand>, DeletePropertyCommandHandler>();
        services
            .AddScoped<ICommandHandler<AddPropertyToProductCommand>, AddPropertyToProductCommandHandler>();
        services
            .AddScoped<ICommandHandler<CreateValueCommand, ValueModel>, CreateValueCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteValueCommand>, DeleteValueCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateValueCommand, ValueModel>, UpdateValueCommandHandler>();
        services.AddScoped<ICommandHandler<UploadImageCommand, ImageModel>, UploadImageCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteImageCommand>, DeleteImageCommandHandler>();


        services.AddScoped<IQueryHandler<GetProductsQuery, ICollection<ProductModel>>, GetProductsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductModel>, GetProductByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>,
            GetPropertiesByProductIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertiesQuery, ICollection<PropertyModel>>, GetPropertiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetValuesByPropertyIdQuery, PropertyModel>,
            GetValuesByPropertyIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetValuesQuery, ICollection<ValueModel>>, GetValuesQueryHandler>();
        services.AddScoped<IQueryHandler<GetPropertyByIdQuery, PropertyModel>, GetPropertyByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetImagesQuery, ICollection<ImageModel>>, GetImagesQueryHandler>();
        services.AddScoped<IQueryHandler<GetImagesByProductIdQuery, ICollection<ImageModel>>,
            GetImagesByProductIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetImageByIdQuery, ImageModel>, GetImageByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetFilteredProductsQuery, ICollection<ProductModel>>,
            GetFilteredProductsQueryHandler>();
        services.AddScoped<IQueryHandler<GetValueByIdQuery , ValueModel>, GetValueByIdQueryHandler>();

        services.AddSingleton(MapsterConfig.Configure());
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}