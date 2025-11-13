using DroneBuilder.API.Endpoints;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.API;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true,
            reloadOnChange: true);

        builder.Services.AddSwaggerGen();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services
            .AddIdentity<User, IdentityRole<Guid>>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddAuthorization();

        builder.Services.AddOpenApi();

        builder.Services.AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddAuth(builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapUserEndpoints();
        app.MapProductEndpoints();

        app.Run();
    }
}