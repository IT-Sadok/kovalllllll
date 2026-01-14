using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Infrastructure.MessageBroker.Configuration;
using DroneBuilder.Infrastructure.MessageBroker.Services;
using DroneBuilder.Infrastructure.Options;
using DroneBuilder.Infrastructure.Repositories;
using DroneBuilder.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DroneBuilder.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.Configure<AzureStorageConfig>(configuration.GetSection("AzureStorage"));

        services.Configure<RabbitMqConfiguration>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value);

        services.Configure<MessageQueuesConfiguration>(configuration.GetSection("MessageQueues"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MessageQueuesConfiguration>>().Value);


        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
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

        return services;
    }

    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        var assembly = typeof(InfrastructureExtensions).Assembly;

        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEventHandler).IsAssignableFrom(t))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(typeof(IEventHandler), handlerType);
        }

        return services;
    }
}