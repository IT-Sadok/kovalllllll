using System.Reflection;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.Validation.Options;
using DroneBuilder.Infrastructure.MessageBroker.Configuration;
using DroneBuilder.Infrastructure.MessageBroker.Services;
using DroneBuilder.Infrastructure.Options;
using DroneBuilder.Infrastructure.Repositories;
using DroneBuilder.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DroneBuilder.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
                               ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        services.AddValidatorsFromAssembly(typeof(InfrastructureExtensions).Assembly);

        services.AddOptions<AzureStorageConfig>()
            .Bind(configuration.GetSection("AzureStorage"))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddOptions<RabbitMqConfiguration>()
            .Bind(configuration.GetSection("RabbitMQ"))
            .ValidateFluentValidation()
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value);

        services.AddOptions<MessageQueuesConfiguration>()
            .Bind(configuration.GetSection("MessageQueues"))
            .ValidateFluentValidation()
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MessageQueuesConfiguration>>().Value);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICatalogSpecificationRepository, CatalogSpecificationRepository>();
        services.AddScoped<ICatalogMetadataRepository, CatalogMetadataRepository>();
        services.AddScoped<ICatalogImportRepository, CatalogImportRepository>();
        services.AddScoped<ICompatibilityRepository, CompatibilityRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IValueRepository, ValueRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAzureStorageService, AzureStorageService>();

        services.AddScoped<IOutboxEventService, OutboxEventService>();

        services.AddEventHandlers();

        services.AddHostedService<OutboxProcessorHostedService>();
        services.AddHostedService<EventConsumerHostedService>();
        services.AddHostedService<ExpiredInventoryReservationHostedService>();

        return services;
    }

    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        Assembly assembly = typeof(InfrastructureExtensions).Assembly;

        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEventHandler).IsAssignableFrom(t))
            .ToList();

        foreach (Type? handlerType in handlerTypes)
        {
            services.AddScoped(typeof(IEventHandler), handlerType);
        }

        return services;
    }
}
