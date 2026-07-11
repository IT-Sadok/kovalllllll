using DroneBuilder.API.Authorization;
using DroneBuilder.API.Documentation;
using DroneBuilder.API.Endpoints;
using DroneBuilder.API.Extensions;
using DroneBuilder.API.Middleware;
using DroneBuilder.Application;
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
        builder.Services.AddOpenApiConfig();

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

        await app.InitializeDatabaseAsync();

        app.UseExceptionHandler();
        app.MapOpenApiUi();

        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();

        app.UseSpaStaticFiles();
        app.MapApplicationEndpoints();
        app.MapSpaFallback();

        await app.RunAsync();
    }
}
