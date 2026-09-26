using System.Net.Http.Headers;
using System.Reflection;
using Azure.Storage.Blobs;
using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Common.Validation.Options;
using DroneBuilder.Infrastructure.Imports;
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
using Stripe;

namespace DroneBuilder.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
                               ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString,
            npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        services.AddValidatorsFromAssembly(typeof(InfrastructureExtensions).Assembly);

        services.AddOptions<AzureStorageConfig>()
            .Bind(configuration.GetSection("AzureStorage"))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddOptions<ResendOptions>()
            .Bind(configuration.GetSection("Resend"))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddOptions<RabbitMqConfiguration>()
            .Bind(configuration.GetSection("RabbitMQ"))
            .ValidateFluentValidation()
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value);

        services.AddOptions<CartReservationOptions>()
            .Bind(configuration.GetSection("CartReservation"))
            .ValidateFluentValidation()
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<CartReservationOptions>>().Value);

        services.AddOptions<MessageQueuesConfiguration>()
            .Bind(configuration.GetSection("MessageQueues"))
            .ValidateFluentValidation()
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MessageQueuesConfiguration>>().Value);

        services.AddOptions<RaceDayQuadsImportOptions>()
            .Bind(configuration.GetSection("RaceDayQuadsImport"))
            .ValidateFluentValidation()
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RaceDayQuadsImportOptions>>().Value);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IImportRunRepository, ImportRunRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IBuildRepository, BuildRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IJwtService, JwtService>();

        services.AddSingleton(sp =>
            new BlobServiceClient(sp.GetRequiredService<IOptions<AzureStorageConfig>>().Value.ConnectionString));

        services.AddScoped<IAzureStorageService, AzureStorageService>();

        services.AddHttpClient<IEmailSender, ResendEmailService>((sp, client) =>
        {
            ResendOptions resendOptions = sp.GetRequiredService<IOptions<ResendOptions>>().Value;

            client.BaseAddress = new Uri("https://api.resend.com/");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", resendOptions.ApiKey);
        });

        services.AddScoped<IOutboxEventService, OutboxEventService>();

        services.AddPaymentGateway(configuration);

        services.AddEventHandlers();

        services.AddHostedService<OutboxProcessorHostedService>();
        services.AddHostedService<EventConsumerHostedService>();
        services.AddHostedService<ExpiredCartReservationsHostedService>();

        services.AddSingleton<ImportQueue>();
        services.AddSingleton<IImportQueue>(sp => sp.GetRequiredService<ImportQueue>());
        services.AddHttpClient<IRaceDayQuadsClient, RaceDayQuadsClient>((sp, client) =>
        {
            client.BaseAddress = new Uri(sp.GetRequiredService<RaceDayQuadsImportOptions>().BaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DroneBuilderCatalogImporter/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddHostedService<ImportWorkerHostedService>();

        return services;
    }

    private static IServiceCollection AddPaymentGateway(this IServiceCollection services, IConfiguration configuration)
    {
        string provider = configuration["Payments:Provider"] ?? "Stripe";

        if (string.Equals(provider, "Fake", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IPaymentGateway, FakePaymentGateway>();
            return services;
        }

        if (!string.Equals(provider, "Stripe", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unknown Payments:Provider '{provider}'. Use 'Stripe' or 'Fake'.");
        }

        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection("Payments:Stripe"))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddSingleton<IStripeClient>(sp =>
            new StripeClient(sp.GetRequiredService<IOptions<StripeOptions>>().Value.SecretKey));

        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

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
