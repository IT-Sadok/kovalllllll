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

        var rabbitMqSettings = new RabbitMqConfiguration();
        configuration.GetSection("RabbitMQ").Bind(rabbitMqSettings);
        services.AddSingleton(rabbitMqSettings);

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

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOutboxEventService, OutboxEventService>();
        services.AddHostedService<OutboxProcessorHostedService>();
        services.AddHostedService<EventConsumerHostedService>();

        return services;
    }
}