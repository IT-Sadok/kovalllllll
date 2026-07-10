using DroneBuilder.API.Authorization;
using DroneBuilder.API.Documentation;
using DroneBuilder.API.Endpoints;
using DroneBuilder.API.Extensions;
using DroneBuilder.API.Middleware;
using DroneBuilder.Application;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure;
using DroneBuilder.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace DroneBuilder.API;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services
            .AddIdentity<User, IdentityRole<Guid>>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Admin, policy => policy.RequireRole("Admin"))
            .AddPolicy(PolicyNames.User, policy => policy.RequireRole("User"));

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "DroneBuilder API",
                    Version = "v1",
                    Description = "API for managing drone products, configurations, orders and warehouse operations.",
                    Contact = new OpenApiContact
                    {
                        Name = "DroneBuilder Dev Team",
                        Email = "dev@dronebuilder.io"
                    }
                };

                return Task.CompletedTask;
            });

            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<BearerSecurityOperationTransformer>();
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddAuth(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        WebApplication app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;

            ApplicationDbContext dbContext = services.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            await IdentitySeeder.SeedRolesAndAdminAsync(services);
        }

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapScalarApiReference(options =>
            {
                options.Title = "DroneBuilder API";
                options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHttpsRedirection();

        string webRootSegment = "wwwroot".TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string webRootPath = Path.Join(app.Environment.ContentRootPath, webRootSegment);
        bool hasSpaAssets = Directory.Exists(webRootPath) && File.Exists(Path.Combine(webRootPath, "index.html"));

        if (hasSpaAssets)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        app.MapUserEndpoints()
            .MapProductEndpoints()
            .MapPropertyEndpoints()
            .MapValueEndpoints()
            .MapImageEndpoints()
            .MapCartEndpoints()
            .MapWarehouseEndpoints()
            .MapOrderEndpoints();

        if (hasSpaAssets)
        {
            app.MapFallbackToFile("index.html");
        }

        await app.RunAsync();
    }
}
