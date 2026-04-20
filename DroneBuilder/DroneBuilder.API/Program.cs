using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints;
using DroneBuilder.API.Extensions;
using DroneBuilder.API.Middleware;
using DroneBuilder.Application;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure;
using DroneBuilder.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.API;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true,
            reloadOnChange: true);

        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services
            .AddIdentity<User, IdentityRole<Guid>>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Admin, policy => policy.RequireRole("Admin"))
            .AddPolicy(PolicyNames.User, policy => policy.RequireRole("User"));

        builder.Services.AddOpenApi();

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

        // Allow large file uploads (up to 100 MB) - needed for Azure App Service
        builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
        });
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 104_857_600; // 100 MB
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            await IdentitySeeder.SeedRolesAndAdminAsync(services);
        }

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowAll");  // Must be BEFORE UseAuthentication so OPTIONS preflight gets CORS headers

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHttpsRedirection();

        var webRootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        var hasSpaAssets = Directory.Exists(webRootPath) && File.Exists(Path.Combine(webRootPath, "index.html"));

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

        app.Run();
    }
}