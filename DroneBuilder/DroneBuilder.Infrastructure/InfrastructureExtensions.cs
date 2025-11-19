using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Repositories;
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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IValueRepository, ValueRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAzureStorageService, AzureStorageService>();

        return services;
    }
}